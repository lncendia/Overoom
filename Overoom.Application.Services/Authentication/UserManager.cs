﻿using Microsoft.AspNetCore.Identity;
using Overoom.Application.Abstractions.Authentication.DTOs;
using Overoom.Application.Abstractions.Authentication.Entities;
using Overoom.Application.Abstractions.Authentication.Exceptions;
using Overoom.Application.Abstractions.Authentication.Interfaces;
using Overoom.Application.Abstractions.Common.Exceptions;
using Overoom.Domain.Abstractions.Repositories.UnitOfWorks;
using Overoom.Domain.Specifications;
using Overoom.Domain.Specifications.Abstractions;
using Overoom.Domain.Users.Entities;
using Overoom.Domain.Users.Exceptions;
using Overoom.Domain.Users.Specifications;
using Overoom.Domain.Users.Specifications.Visitor;

namespace Overoom.Application.Services.Authentication;

public class UserManager : IUserManager
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<UserData> _userManager;
    private readonly IPasswordHasher<UserData> _passwordHasher;
    private readonly IPasswordValidator<UserData> _passwordValidator;

    public UserManager(IUnitOfWork unitOfWork, UserManager<UserData> userManager,
        IPasswordHasher<UserData> passwordHasher, IPasswordValidator<UserData> passwordValidator)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
    }

    public async Task<List<UserDto>> FindAsync(SearchQuery query)
    {
        //todo: check all for exceptions
        ISpecification<User, IUserSpecificationVisitor>? spec = null;
        if (!string.IsNullOrEmpty(query.Email)) spec = new UserByEmailSpecification(query.Email);
        if (!string.IsNullOrEmpty(query.Name))
        {
            spec = spec == null
                ? new UserByNameSpecification(query.Name)
                : new AndSpecification<User, IUserSpecificationVisitor>(spec,
                    new UserByNameSpecification(query.Name));
        }

        var users = await _unitOfWork.UserRepository.Value.FindAsync(spec, null, (query.Page - 1) * 30,
            30);
        return users.Select(x => new UserDto(x.Name, x.Email, x.Id)).ToList();
    }

    public async Task<UserDto> GetAsync(Guid userId)
    {
        var user = await _unitOfWork.UserRepository.Value.GetAsync(userId);
        if (user == null) throw new UserNotFoundException();
        return new UserDto(user.Name, user.Email, user.Id);
    }

    public async Task EditAsync(EditUserDto editData)
    {
        var user = await _unitOfWork.UserRepository.Value.GetAsync(editData.Id);
        if (user == null) throw new UserNotFoundException();
        var userApplication = await _userManager.FindByEmailAsync(user.Email);
        if (userApplication == null) throw new UserNotFoundException();
        if (!string.IsNullOrEmpty(editData.Email))
        {
            user.Email = editData.Email;
            var result = await _userManager.SetEmailAsync(userApplication, editData.Email);
            if (!result.Succeeded) throw new UserAlreadyExistException();
            userApplication.EmailConfirmed = true;
            await _userManager.UpdateAsync(userApplication);
        }

        if (!string.IsNullOrEmpty(editData.Username))
        {
            user.Name = editData.Username;
            await _userManager.SetUserNameAsync(userApplication, editData.Username);
        }

        await _unitOfWork.UserRepository.Value.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(string email, string password)
    {
        var userApplication = await _userManager.FindByEmailAsync(email);
        if (userApplication == null) throw new UserNotFoundException();
        var result = await _passwordValidator.ValidateAsync(_userManager, userApplication, password);
        CheckResult(result);
        userApplication.PasswordHash = _passwordHasher.HashPassword(userApplication, password);
        await _userManager.UpdateAsync(userApplication);
    }

    public async Task<UserData> GetAuthenticationDataAsync(Guid userId)
    {
        var user = await _unitOfWork.UserRepository.Value.GetAsync(userId);
        if (user == null) throw new UserNotFoundException();
        var userApplication = await _userManager.FindByEmailAsync(user.Email);
        if (userApplication == null) throw new UserNotFoundException();
        return userApplication;
    }
    
    
    private static void CheckResult(IdentityResult result)
    {
        if (result.Succeeded) return;
        Exception ex = result.Errors.First().Code switch
        {
            "MailUsed" => new UserAlreadyExistException(),
            "MailFormat" => new EmailFormatException(),
            "NameLength" => new NicknameLengthException(),
            "NameFormat" => new NicknameFormatException(),
            "PasswordLength" => new PasswordLengthException(),
            "PasswordFormat" => new PasswordFormatException(),
            _ => new UserCreationException(result.Errors.First().Description)
        };
        throw ex;
    }
}