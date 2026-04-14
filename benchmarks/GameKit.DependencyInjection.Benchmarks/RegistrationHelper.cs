using Microsoft.Extensions.DependencyInjection;
using GkDI = GameKit.DependencyInjection;

namespace GameKit.DependencyInjection.Benchmarks;

public static class RegistrationHelper
{
    public static GkDI.ServiceCollection BuildGameKitCollection()
    {
        GkDI.ServiceCollection c = new();

        // Leaves
        c.AddSingleton<Leaf00>(); c.AddSingleton<Leaf01>(); c.AddSingleton<Leaf02>(); c.AddSingleton<Leaf03>(); c.AddSingleton<Leaf04>();
        c.AddSingleton<Leaf05>(); c.AddSingleton<Leaf06>(); c.AddSingleton<Leaf07>(); c.AddSingleton<Leaf08>(); c.AddSingleton<Leaf09>();
        c.AddSingleton<Leaf10>(); c.AddSingleton<Leaf11>(); c.AddSingleton<Leaf12>(); c.AddSingleton<Leaf13>(); c.AddSingleton<Leaf14>();
        c.AddSingleton<Leaf15>(); c.AddSingleton<Leaf16>(); c.AddSingleton<Leaf17>(); c.AddSingleton<Leaf18>(); c.AddSingleton<Leaf19>();

        // Chains
        c.AddSingleton<Chain00>(); c.AddSingleton<Chain01>(); c.AddSingleton<Chain02>(); c.AddSingleton<Chain03>(); c.AddSingleton<Chain04>();
        c.AddSingleton<Chain05>(); c.AddSingleton<Chain06>(); c.AddSingleton<Chain07>(); c.AddSingleton<Chain08>(); c.AddSingleton<Chain09>();
        c.AddSingleton<Chain10>(); c.AddSingleton<Chain11>(); c.AddSingleton<Chain12>(); c.AddSingleton<Chain13>(); c.AddSingleton<Chain14>();
        c.AddSingleton<Chain15>(); c.AddSingleton<Chain16>(); c.AddSingleton<Chain17>(); c.AddSingleton<Chain18>(); c.AddSingleton<Chain19>();

        // Fans
        c.AddSingleton<Fan00>(); c.AddSingleton<Fan01>(); c.AddSingleton<Fan02>(); c.AddSingleton<Fan03>(); c.AddSingleton<Fan04>();
        c.AddSingleton<Fan05>(); c.AddSingleton<Fan06>(); c.AddSingleton<Fan07>(); c.AddSingleton<Fan08>(); c.AddSingleton<Fan09>();
        c.AddSingleton<Fan10>(); c.AddSingleton<Fan11>(); c.AddSingleton<Fan12>(); c.AddSingleton<Fan13>(); c.AddSingleton<Fan14>();
        c.AddSingleton<Fan15>(); c.AddSingleton<Fan16>(); c.AddSingleton<Fan17>(); c.AddSingleton<Fan18>(); c.AddSingleton<Fan19>();

        // Diamonds
        c.AddSingleton<Diamond00>(); c.AddSingleton<Diamond01>(); c.AddSingleton<Diamond02>(); c.AddSingleton<Diamond03>(); c.AddSingleton<Diamond04>();
        c.AddSingleton<Diamond05>(); c.AddSingleton<Diamond06>(); c.AddSingleton<Diamond07>(); c.AddSingleton<Diamond08>(); c.AddSingleton<Diamond09>();
        c.AddSingleton<Diamond10>(); c.AddSingleton<Diamond11>(); c.AddSingleton<Diamond12>(); c.AddSingleton<Diamond13>(); c.AddSingleton<Diamond14>();
        c.AddSingleton<Diamond15>(); c.AddSingleton<Diamond16>(); c.AddSingleton<Diamond17>(); c.AddSingleton<Diamond18>(); c.AddSingleton<Diamond19>();

        // Tops
        c.AddSingleton<Top00>(); c.AddSingleton<Top01>(); c.AddSingleton<Top02>(); c.AddSingleton<Top03>(); c.AddSingleton<Top04>();
        c.AddSingleton<Top05>(); c.AddSingleton<Top06>(); c.AddSingleton<Top07>(); c.AddSingleton<Top08>(); c.AddSingleton<Top09>();
        c.AddSingleton<Top10>(); c.AddSingleton<Top11>(); c.AddSingleton<Top12>(); c.AddSingleton<Top13>(); c.AddSingleton<Top14>();
        c.AddSingleton<Top15>(); c.AddSingleton<Top16>(); c.AddSingleton<Top17>(); c.AddSingleton<Top18>(); c.AddSingleton<Top19>();

        return c;
    }

    public static Microsoft.Extensions.DependencyInjection.ServiceCollection BuildMediCollection()
    {
        Microsoft.Extensions.DependencyInjection.ServiceCollection c = new();

        // Leaves
        c.AddSingleton<Leaf00>(); c.AddSingleton<Leaf01>(); c.AddSingleton<Leaf02>(); c.AddSingleton<Leaf03>(); c.AddSingleton<Leaf04>();
        c.AddSingleton<Leaf05>(); c.AddSingleton<Leaf06>(); c.AddSingleton<Leaf07>(); c.AddSingleton<Leaf08>(); c.AddSingleton<Leaf09>();
        c.AddSingleton<Leaf10>(); c.AddSingleton<Leaf11>(); c.AddSingleton<Leaf12>(); c.AddSingleton<Leaf13>(); c.AddSingleton<Leaf14>();
        c.AddSingleton<Leaf15>(); c.AddSingleton<Leaf16>(); c.AddSingleton<Leaf17>(); c.AddSingleton<Leaf18>(); c.AddSingleton<Leaf19>();

        // Chains
        c.AddSingleton<Chain00>(); c.AddSingleton<Chain01>(); c.AddSingleton<Chain02>(); c.AddSingleton<Chain03>(); c.AddSingleton<Chain04>();
        c.AddSingleton<Chain05>(); c.AddSingleton<Chain06>(); c.AddSingleton<Chain07>(); c.AddSingleton<Chain08>(); c.AddSingleton<Chain09>();
        c.AddSingleton<Chain10>(); c.AddSingleton<Chain11>(); c.AddSingleton<Chain12>(); c.AddSingleton<Chain13>(); c.AddSingleton<Chain14>();
        c.AddSingleton<Chain15>(); c.AddSingleton<Chain16>(); c.AddSingleton<Chain17>(); c.AddSingleton<Chain18>(); c.AddSingleton<Chain19>();

        // Fans
        c.AddSingleton<Fan00>(); c.AddSingleton<Fan01>(); c.AddSingleton<Fan02>(); c.AddSingleton<Fan03>(); c.AddSingleton<Fan04>();
        c.AddSingleton<Fan05>(); c.AddSingleton<Fan06>(); c.AddSingleton<Fan07>(); c.AddSingleton<Fan08>(); c.AddSingleton<Fan09>();
        c.AddSingleton<Fan10>(); c.AddSingleton<Fan11>(); c.AddSingleton<Fan12>(); c.AddSingleton<Fan13>(); c.AddSingleton<Fan14>();
        c.AddSingleton<Fan15>(); c.AddSingleton<Fan16>(); c.AddSingleton<Fan17>(); c.AddSingleton<Fan18>(); c.AddSingleton<Fan19>();

        // Diamonds
        c.AddSingleton<Diamond00>(); c.AddSingleton<Diamond01>(); c.AddSingleton<Diamond02>(); c.AddSingleton<Diamond03>(); c.AddSingleton<Diamond04>();
        c.AddSingleton<Diamond05>(); c.AddSingleton<Diamond06>(); c.AddSingleton<Diamond07>(); c.AddSingleton<Diamond08>(); c.AddSingleton<Diamond09>();
        c.AddSingleton<Diamond10>(); c.AddSingleton<Diamond11>(); c.AddSingleton<Diamond12>(); c.AddSingleton<Diamond13>(); c.AddSingleton<Diamond14>();
        c.AddSingleton<Diamond15>(); c.AddSingleton<Diamond16>(); c.AddSingleton<Diamond17>(); c.AddSingleton<Diamond18>(); c.AddSingleton<Diamond19>();

        // Tops
        c.AddSingleton<Top00>(); c.AddSingleton<Top01>(); c.AddSingleton<Top02>(); c.AddSingleton<Top03>(); c.AddSingleton<Top04>();
        c.AddSingleton<Top05>(); c.AddSingleton<Top06>(); c.AddSingleton<Top07>(); c.AddSingleton<Top08>(); c.AddSingleton<Top09>();
        c.AddSingleton<Top10>(); c.AddSingleton<Top11>(); c.AddSingleton<Top12>(); c.AddSingleton<Top13>(); c.AddSingleton<Top14>();
        c.AddSingleton<Top15>(); c.AddSingleton<Top16>(); c.AddSingleton<Top17>(); c.AddSingleton<Top18>(); c.AddSingleton<Top19>();

        return c;
    }
}
