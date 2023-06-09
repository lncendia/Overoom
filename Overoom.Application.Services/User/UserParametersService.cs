﻿using Overoom.Application.Abstractions.User.Exceptions;
using Overoom.Application.Abstractions.User.Interfaces;
using Overoom.Domain.Abstractions.Repositories.UnitOfWorks;
using Overoom.Domain.User.Specifications;

namespace Overoom.Application.Services.User;

public class UserParametersService : IUserParametersService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserThumbnailService _photoManager;

    public UserParametersService(IUnitOfWork unitOfWork, IUserThumbnailService photoManager)
    {
        _unitOfWork = unitOfWork;
        _photoManager = photoManager;
    }

    public async Task<Domain.User.Entities.User> GetAsync(string email)
    {
        var user = (await _unitOfWork.UserRepository.Value.FindAsync(new UserByEmailSpecification(email), null, 0, 1))
            .FirstOrDefault();
        if (user == null) throw new UserNotFoundException();
        return user;
    }

    public async Task ChangeNameAsync(string email, string name)
    {
        var user = (await _unitOfWork.UserRepository.Value.FindAsync(new UserByEmailSpecification(email), null, 0, 1))
            .FirstOrDefault();
        if (user == null) throw new UserNotFoundException();
        user.Name = name;
        await _unitOfWork.UserRepository.Value.UpdateAsync(user);
        await _unitOfWork.SaveAsync();
    }


    public async Task ChangeAvatarAsync(string email, Stream avatar)
    {
        var user = (await _unitOfWork.UserRepository.Value.FindAsync(new UserByEmailSpecification(email), null, 0, 1))
            .FirstOrDefault();
        if (user == null) throw new UserNotFoundException();
        var t1 = _photoManager.DeleteAsync(user.AvatarUri);
        var t2 = _photoManager.SaveAsync(avatar);
        await Task.WhenAll(t1, t2);
        user.AvatarUri = t2.Result;
        await _unitOfWork.UserRepository.Value.UpdateAsync(user);
        await _unitOfWork.SaveAsync();
    }
}