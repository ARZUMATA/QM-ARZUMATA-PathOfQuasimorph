using HarmonyLib;
using System;
using System.Reflection.Emit;

public static class CodeMatcherExtensions
{
    public static byte GetLocalIndex(this CodeInstruction inst)
    {
        if (inst.opcode == OpCodes.Stloc_0 || inst.opcode == OpCodes.Ldloc_0)
            return 0;
        if (inst.opcode == OpCodes.Stloc_1 || inst.opcode == OpCodes.Ldloc_1)
            return 1;
        if (inst.opcode == OpCodes.Stloc_2 || inst.opcode == OpCodes.Ldloc_2)
            return 2;
        if (inst.opcode == OpCodes.Stloc_3 || inst.opcode == OpCodes.Ldloc_3)
            return 3;

        if (inst.opcode == OpCodes.Stloc_S || inst.opcode == OpCodes.Ldloc_S)
        {
            if (inst.operand is LocalBuilder lb)
                return (byte)lb.LocalIndex;
            if (inst.operand is int i)
                return (byte)i;
            if (inst.operand is byte b)
                return b;
        }

        throw new InvalidOperationException("Instruction is not a known ldloc/stloc opcode.");
    }

    public static byte GetLocalIndex(this CodeMatcher matcher)
    {
        return GetLocalIndex(matcher.Instruction);
    }
}