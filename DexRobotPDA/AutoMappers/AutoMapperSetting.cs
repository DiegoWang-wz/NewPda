using AutoMapper;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;

namespace DexRobotPDA.AutoMappers;

public class AutoMapperSetting : Profile
{
    public AutoMapperSetting()
    {
        // byte转bool：仅1为true，0为false，其他值也视为false
        CreateMap<byte, bool>().ConvertUsing(src => src == 1);

        // bool转byte：true→1，false→0
        CreateMap<bool, byte>().ConvertUsing(src => src ? (byte)1 : (byte)0);

        CreateMap<TeamModel, TeamDto>().ReverseMap();
        CreateMap<EmployeeModel, EmployeeDto>().ReverseMap();
        CreateMap<ProductTaskModel, ProductTaskDto>().ReverseMap();
        CreateMap<ProductionBatchModel, ProductionBatchDto>().ReverseMap();
        CreateMap<MotorModel, MotorDto>().ReverseMap();
        CreateMap<FingerModel, FingerDto>().ReverseMap();
        CreateMap<SplitModel, SplitDto>().ReverseMap();
        CreateMap<PalmModel, PalmDto>().ReverseMap();
        CreateMap<MaterialModel, MaterialDto>().ReverseMap();
        CreateMap<MotorWormDetectModel, MotorWormDetectDto>().ReverseMap();
        CreateMap<SplitWormDetectModel,SplitWormDetectDto>().ReverseMap();
        CreateMap<SplitCalibrateDetectModel, SplitCalibrateDetectDto>().ReverseMap();
        CreateMap<FingerCalibrateDetectModel, FingerCalibrateDetectDto>().ReverseMap();
        CreateMap<PalmCalibrateDetectModel, PalmCalibrateDetectDto>().ReverseMap();
        CreateMap<EventLogModel, EventLogDto>().ReverseMap();
        CreateMap<AddTaskDto, ProductTaskModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore()) // 忽略自增主键
            .ForMember(dest => dest.status, opt => opt.Ignore())
            .ForMember(dest => dest.created_at, opt => opt.MapFrom(src => DateTime.Now)) // 使用当前时间
            .ForMember(dest => dest.updated_at, opt => opt.MapFrom(src => DateTime.Now)) // 使用当前时间
            .ForMember(dest => dest.Assignee, opt => opt.Ignore()) // 忽略导航属性
            .ForMember(dest => dest.ProductionBatches, opt => opt.Ignore()); // 忽略导航属性

        CreateMap<EmployeeModel, UserDto>();

        CreateMap<AddMotorDto, MotorModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore())
            .ForMember(dest => dest.finger_id, opt => opt.Ignore())
            .ForMember(dest => dest.updated_at, opt => opt.Ignore())
            .ForMember(dest => dest.TaskModel, opt => opt.Ignore())
            // .ForMember(dest => dest.WormMaterial, opt => opt.Ignore())
            // .ForMember(dest => dest.AdhesiveMaterial, opt => opt.Ignore())
            .ForMember(dest => dest.Operator, opt => opt.Ignore())
            .ForMember(dest => dest.Finger, opt => opt.Ignore());

        CreateMap<AddFingerDto, FingerModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore())
            .ForMember(dest => dest.palm_id, opt => opt.MapFrom(src => (string)null))
            .ForMember(dest => dest.updated_at, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.created_at, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.TaskModel, opt => opt.Ignore())
            .ForMember(dest => dest.Operator, opt => opt.Ignore())
            .ForMember(dest => dest.Palm, opt => opt.Ignore())
            .ForMember(dest => dest.Motors, opt => opt.Ignore());

        CreateMap<AddSplitDto, SplitModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore())
            .ForMember(dest => dest.palm_id, opt => opt.MapFrom(src => (string)null))
            .ForMember(dest => dest.updated_at, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.created_at, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.TaskModel, opt => opt.Ignore())
            .ForMember(dest => dest.Operator, opt => opt.Ignore())
            .ForMember(dest => dest.Palm, opt => opt.Ignore());

        CreateMap<AddPalmDto, PalmModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore())
            .ForMember(dest => dest.updated_at, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.created_at, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.TaskModel, opt => opt.Ignore())
            .ForMember(dest => dest.Operator, opt => opt.Ignore())
            .ForMember(dest => dest.Fingers, opt => opt.Ignore());
        
        CreateMap<UpdateDetect1Dto, MotorWormDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<SplitWormDetectCreateDto,SplitWormDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<SplitCalibrateDetectCreateDto,SplitCalibrateDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<FingerCalibrateDetectCreateDto,FingerCalibrateDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<PalmCalibrateDetectCreateDto,PalmCalibrateDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
                
        CreateMap<AddEventLogDto,EventLogModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
    }
}