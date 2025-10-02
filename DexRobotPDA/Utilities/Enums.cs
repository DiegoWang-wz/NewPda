using System;

namespace DexRobotPDA.Utilities
{
    /// <summary>
    /// 操作员状态枚举（对应 operator.status）
    /// </summary>
    public enum OperatorStatus
    {
        active,
        inactive
    }

    /// <summary>
    /// 物料状态枚举（对应 motor/worm_gear/finger_shell/palm_shell.status）
    /// </summary>
    public enum MaterialStatus
    {
        in_stock,    // 在库
        bound,       // 已绑定
        installed,   // 已安装
        defective    // 已损坏
    }

    /// <summary>
    /// 电机蜗杆绑定状态枚举（对应 motor_worm_binding.status）
    /// </summary>
    public enum BindingStatus
    {
        active,      // 绑定有效
        unbound      // 已解绑
    }

    /// <summary>
    /// 组装状态枚举（对应 finger_assembly/palm_assembly.status）
    /// </summary>
    public enum AssemblyStatus
    {
        active,        // 组装有效
        disassembled   // 已拆解
    }

    /// <summary>
    /// 订单状态枚举（对应 orders.status）
    /// </summary>
    public enum OrderStatus
    {
        pending,        // 待处理
        in_production,  // 生产中
        quality_check,  // 质检中
        shipped,        // 已发货
        delivered,      // 已交付
        cancelled       // 已取消
    }

    /// <summary>
    /// 手指类型枚举（对应 finger_shell.finger_type）
    /// </summary>
    public enum FingerType
    {
        thumb,   // 拇指
        index,   // 食指
        middle,  // 中指
        ring,    // 无名指
        pinky    // 小指
    }
}