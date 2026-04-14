using Microsoft.Extensions.DependencyInjection;
using GkDI = GameKit.DependencyInjection;

namespace GameKit.DependencyInjection.Benchmarks;

public static class RegistrationHelper
{
    public static GkDI.ServiceCollection BuildGameKitCollection()
    {
        GkDI.ServiceCollection c = new();

        // Leaves
        c.RegisterType<Leaf00>(); c.RegisterType<Leaf01>(); c.RegisterType<Leaf02>(); c.RegisterType<Leaf03>(); c.RegisterType<Leaf04>();
        c.RegisterType<Leaf05>(); c.RegisterType<Leaf06>(); c.RegisterType<Leaf07>(); c.RegisterType<Leaf08>(); c.RegisterType<Leaf09>();
        c.RegisterType<Leaf10>(); c.RegisterType<Leaf11>(); c.RegisterType<Leaf12>(); c.RegisterType<Leaf13>(); c.RegisterType<Leaf14>();
        c.RegisterType<Leaf15>(); c.RegisterType<Leaf16>(); c.RegisterType<Leaf17>(); c.RegisterType<Leaf18>(); c.RegisterType<Leaf19>();

        // Chains
        c.RegisterType<Chain00>(); c.RegisterType<Chain01>(); c.RegisterType<Chain02>(); c.RegisterType<Chain03>(); c.RegisterType<Chain04>();
        c.RegisterType<Chain05>(); c.RegisterType<Chain06>(); c.RegisterType<Chain07>(); c.RegisterType<Chain08>(); c.RegisterType<Chain09>();
        c.RegisterType<Chain10>(); c.RegisterType<Chain11>(); c.RegisterType<Chain12>(); c.RegisterType<Chain13>(); c.RegisterType<Chain14>();
        c.RegisterType<Chain15>(); c.RegisterType<Chain16>(); c.RegisterType<Chain17>(); c.RegisterType<Chain18>(); c.RegisterType<Chain19>();

        // Fans
        c.RegisterType<Fan00>(); c.RegisterType<Fan01>(); c.RegisterType<Fan02>(); c.RegisterType<Fan03>(); c.RegisterType<Fan04>();
        c.RegisterType<Fan05>(); c.RegisterType<Fan06>(); c.RegisterType<Fan07>(); c.RegisterType<Fan08>(); c.RegisterType<Fan09>();
        c.RegisterType<Fan10>(); c.RegisterType<Fan11>(); c.RegisterType<Fan12>(); c.RegisterType<Fan13>(); c.RegisterType<Fan14>();
        c.RegisterType<Fan15>(); c.RegisterType<Fan16>(); c.RegisterType<Fan17>(); c.RegisterType<Fan18>(); c.RegisterType<Fan19>();

        // Diamonds
        c.RegisterType<Diamond00>(); c.RegisterType<Diamond01>(); c.RegisterType<Diamond02>(); c.RegisterType<Diamond03>(); c.RegisterType<Diamond04>();
        c.RegisterType<Diamond05>(); c.RegisterType<Diamond06>(); c.RegisterType<Diamond07>(); c.RegisterType<Diamond08>(); c.RegisterType<Diamond09>();
        c.RegisterType<Diamond10>(); c.RegisterType<Diamond11>(); c.RegisterType<Diamond12>(); c.RegisterType<Diamond13>(); c.RegisterType<Diamond14>();
        c.RegisterType<Diamond15>(); c.RegisterType<Diamond16>(); c.RegisterType<Diamond17>(); c.RegisterType<Diamond18>(); c.RegisterType<Diamond19>();

        // Tops
        c.RegisterType<Top00>(); c.RegisterType<Top01>(); c.RegisterType<Top02>(); c.RegisterType<Top03>(); c.RegisterType<Top04>();
        c.RegisterType<Top05>(); c.RegisterType<Top06>(); c.RegisterType<Top07>(); c.RegisterType<Top08>(); c.RegisterType<Top09>();
        c.RegisterType<Top10>(); c.RegisterType<Top11>(); c.RegisterType<Top12>(); c.RegisterType<Top13>(); c.RegisterType<Top14>();
        c.RegisterType<Top15>(); c.RegisterType<Top16>(); c.RegisterType<Top17>(); c.RegisterType<Top18>(); c.RegisterType<Top19>();

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
