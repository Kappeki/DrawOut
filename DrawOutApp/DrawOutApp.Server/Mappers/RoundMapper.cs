using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class RoundMapper
    {
        public static RoundModel ToModel(Round entity, UserModel currentPainter)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var roundModel = new RoundModel(entity.RoundNumber, entity.GameSessionId)
            {
                RoundId = entity.RoundId,
                ActiveWord = entity.ActiveWord,
                IsStealOpportunityActive = entity.IsStealOpportunityActive,
                State = entity.State,
                CurrentPainter = currentPainter,
                RoundChat = entity.RoundChat?.Select(ch=>ch.ToBusinessModel()).ToList(),
                DrawingActions = entity.DrawingActions?.Select(da => da.ToBusinessModel()).ToList()
            };
            return roundModel;
        }
        public static Round ToEntity(RoundModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new Round
            {
                RoundId = model.RoundId,
                GameSessionId = model.GameSessionId,
                RoundNumber = model.RoundNumber,
                ActiveWord = model.ActiveWord,
                CurrentPainterId = model.CurrentPainter?.SessionId, 
                IsStealOpportunityActive = model.IsStealOpportunityActive,
                RoundChat = model.RoundChat?.Select(rc=>new ChatMessage(rc)).ToList(),
                State = model.State,
                DrawingActions = model.DrawingActions?.Select(da=>new DrawingAction(da)).ToList()
            };
        }
    }
}
