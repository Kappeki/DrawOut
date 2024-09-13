using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class DrawingActionMapper : Profile
    {
        public DrawingActionMapper()
        {

            CreateMap<DrawingAction, DrawingActionModel>()
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType.ToString()))
                .ForMember(dest => dest.ToolType, opt => opt.MapFrom(src => src.ToolType.ToString()));

            CreateMap<DrawingActionModel, DrawingAction>()
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => Enum.Parse<ActionType>(src.ActionType!)))
                .ForMember(dest => dest.ToolType, opt => opt.MapFrom(src => Enum.Parse<ToolType>(src.ToolType!)));
        }
    }
}
