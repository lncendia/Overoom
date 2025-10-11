using System.Security.Claims;
using Identix.Application.Abstractions.Entities;

namespace Identix.Application.Abstractions.Services;

/// <summary>
/// Предоставляет абстракцию фабрики для создания <see cref="ClaimsIdentity"/> из пользователя.
/// </summary>
public interface IOpenIdClaimsIdentityFactory
{
  /// <summary>
  /// Создает <see cref="ClaimsIdentity"/> из пользователя асинхронно.
  /// </summary>
  /// <param name="user">Пользователь, на основе которого создается claims identity</param>
  /// <param name="authenticationScheme">Схема аутентификации, связанная с identity</param>
  /// <param name="baseIdentity">Текущий identity для обновления</param>
  /// <returns>Задача, представляющая асинхронную операцию создания, содержащую созданный <see cref="ClaimsIdentity"/>.</returns>
  Task<ClaimsIdentity> CreateAsync(AppUser user, string authenticationScheme, ClaimsIdentity baseIdentity);

  /// <summary>
  /// Получает допустимые destinations (назначения) для указанного claim.
  /// </summary>
  /// <param name="claim">Claim для которого определяются destinations</param>
  /// <returns>Коллекция имен destinations, куда может быть включен claim</returns>
  /// <remarks>
  /// Destinations определяют, в какие типы токенов (access_token, id_token, etc.) 
  /// может быть включен claim в соответствии с политиками безопасности.
  /// </remarks>
  IEnumerable<string> GetDestinations(Claim claim);
}