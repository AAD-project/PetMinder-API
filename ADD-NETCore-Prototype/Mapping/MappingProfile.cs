using Api.DTOs;
using Api.Models;
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mappings
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Pets, opt => opt.MapFrom(src => src.Pets))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
            .ForMember(dest => dest.Reminders, opt => opt.MapFrom(src => src.Reminders));

        CreateMap<UserCreateRequestDto, User>();

        // Pet Mappings
        CreateMap<Pet, PetResponseDto>();
        CreateMap<PetCreateRequestDto, Pet>();

        // HealthData Mappings
        CreateMap<HealthData, HealthDataResponseDto>();

        // Reminder Mappings
        CreateMap<Reminder, ReminderResponseDto>();
        CreateMap<ReminderCreateRequestDto, Reminder>();

        // TodoTask Mappings
        CreateMap<TodoTask, TodoTaskResponseDto>();
        CreateMap<TodoTaskCreateRequestDto, TodoTask>();
    }
}
