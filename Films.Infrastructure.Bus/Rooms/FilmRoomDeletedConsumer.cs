// using Films.Application.Abstractions.Commands.Rooms;
// using MassTransit;
// using MediatR;
// using Overoom.IntegrationEvents.Rooms.Rooms;
//
// namespace Films.Infrastructure.Bus.Rooms;
//
// /// <summary>
// /// Обработчик интеграционного события RoomDeletedIntegrationEvent
// /// </summary>
// /// <param name="mediator">Медиатор</param>
// public class RoomDeletedConsumer(ISender mediator) : IConsumer<RoomDeletedIntegrationEvent>
// {
//     /// <summary>
//     /// Метод обработчик 
//     /// </summary>
//     /// <param name="context">Контекст сообщения</param>
//     public Task Consume(ConsumeContext<RoomDeletedIntegrationEvent> context)
//     {
//         // Получаем данные события
//         var integrationEvent = context.Message;
//
//         // Отправляем команду на обработку события
//         return mediator.Send(new Delete
//         {
//             UserId = integrationEvent.ViewerId,
//             RoomId = integrationEvent.RoomId
//         }, context.CancellationToken);
//     }
// }