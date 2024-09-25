using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    /// <summary>
    /// DEPRECETED SERVICE CLASS NOT USED
    /// </summary>
    public class DrawingActionService : IDrawingActionService
    {
        private readonly IDrawingActionRepo _repository;
        private readonly IMapper _mapper;

        public DrawingActionService(IDrawingActionRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task AddDrawingActionAsync(DrawingActionModel model)
        {
            var action = _mapper.Map<DrawingAction>(model);
            await _repository.AddDrawingActionAsync(action);
        }

        public async Task<List<DrawingActionModel>> GetDrawingActionsSinceAsync(string roomId, long sinceTimestamp, string painterName)
        {
            var actions = await _repository.GetDrawingActionsSinceAsync(roomId, painterName, sinceTimestamp);
            return actions.Select(action => _mapper.Map<DrawingActionModel>(action)).ToList();
        }

        public async Task<List<DrawingActionModel>> GetLastNActionsAsync(string roomId, int n, string painterName)
        {
            var actions = await _repository.GetLastNActionsAsync(roomId, painterName, n);
            return actions.Select(action => _mapper.Map<DrawingActionModel>(action)).ToList();
        }

        public async Task ClearDrawingActionsAsync(string roomId, string painterName)
        {
            await _repository.ClearDrawingActionsAsync(roomId, painterName);
        }

        public async Task UndoStrokeAsync(string roomId, string painterName, string strokeId)
        {
            await _repository.UndoStrokeAsync(roomId, painterName, strokeId);
        }



    }
}
