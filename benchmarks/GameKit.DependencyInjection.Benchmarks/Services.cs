#pragma warning disable CS9113
namespace GameKit.DependencyInjection.Benchmarks;

// Leaf services (no dependencies) — 20 services
public class Leaf00; public class Leaf01; public class Leaf02; public class Leaf03; public class Leaf04;
public class Leaf05; public class Leaf06; public class Leaf07; public class Leaf08; public class Leaf09;
public class Leaf10; public class Leaf11; public class Leaf12; public class Leaf13; public class Leaf14;
public class Leaf15; public class Leaf16; public class Leaf17; public class Leaf18; public class Leaf19;

// Single-dependency chain — 20 services, each depends on previous
public class Chain00(Leaf00 d);
public class Chain01(Chain00 d);
public class Chain02(Chain01 d);
public class Chain03(Chain02 d);
public class Chain04(Chain03 d);
public class Chain05(Chain04 d);
public class Chain06(Chain05 d);
public class Chain07(Chain06 d);
public class Chain08(Chain07 d);
public class Chain09(Chain08 d);
public class Chain10(Chain09 d);
public class Chain11(Chain10 d);
public class Chain12(Chain11 d);
public class Chain13(Chain12 d);
public class Chain14(Chain13 d);
public class Chain15(Chain14 d);
public class Chain16(Chain15 d);
public class Chain17(Chain16 d);
public class Chain18(Chain17 d);
public class Chain19(Chain18 d);

// Fan-out: multiple dependencies — 20 services, each depends on 2-4 leaves
public class Fan00(Leaf00 a, Leaf01 b);
public class Fan01(Leaf02 a, Leaf03 b, Leaf04 c);
public class Fan02(Leaf05 a, Leaf06 b);
public class Fan03(Leaf07 a, Leaf08 b, Leaf09 c);
public class Fan04(Leaf10 a, Leaf11 b, Leaf12 c, Leaf13 d);
public class Fan05(Leaf14 a, Leaf15 b);
public class Fan06(Leaf16 a, Leaf17 b, Leaf18 c);
public class Fan07(Leaf19 a, Leaf00 b);
public class Fan08(Leaf01 a, Leaf02 b, Leaf03 c, Leaf04 d);
public class Fan09(Leaf05 a, Leaf06 b);
public class Fan10(Leaf07 a, Leaf08 b, Leaf09 c);
public class Fan11(Leaf10 a, Leaf11 b);
public class Fan12(Leaf12 a, Leaf13 b, Leaf14 c);
public class Fan13(Leaf15 a, Leaf16 b, Leaf17 c, Leaf18 d);
public class Fan14(Leaf19 a, Leaf00 b);
public class Fan15(Leaf01 a, Leaf02 b, Leaf03 c);
public class Fan16(Leaf04 a, Leaf05 b);
public class Fan17(Leaf06 a, Leaf07 b, Leaf08 c);
public class Fan18(Leaf09 a, Leaf10 b, Leaf11 c, Leaf12 d);
public class Fan19(Leaf13 a, Leaf14 b);

// Diamond: depend on other mid-level services — 20 services
public class Diamond00(Fan00 a, Chain00 b);
public class Diamond01(Fan01 a, Chain01 b);
public class Diamond02(Fan02 a, Chain02 b, Leaf00 c);
public class Diamond03(Fan03 a, Chain03 b);
public class Diamond04(Fan04 a, Chain04 b, Fan00 c);
public class Diamond05(Fan05 a, Chain05 b);
public class Diamond06(Fan06 a, Chain06 b, Diamond00 c);
public class Diamond07(Fan07 a, Chain07 b);
public class Diamond08(Fan08 a, Chain08 b, Diamond01 c);
public class Diamond09(Fan09 a, Chain09 b);
public class Diamond10(Fan10 a, Chain10 b, Diamond02 c, Leaf15 d);
public class Diamond11(Fan11 a, Chain11 b);
public class Diamond12(Fan12 a, Chain12 b, Diamond03 c);
public class Diamond13(Fan13 a, Chain13 b);
public class Diamond14(Fan14 a, Chain14 b, Diamond04 c);
public class Diamond15(Fan15 a, Chain15 b);
public class Diamond16(Fan16 a, Chain16 b, Diamond05 c, Fan10 d);
public class Diamond17(Fan17 a, Chain17 b);
public class Diamond18(Fan18 a, Chain18 b, Diamond06 c);
public class Diamond19(Fan19 a, Chain19 b);

// Top-level aggregators — 20 services depending on diamonds, fans, chains
public class Top00(Diamond00 a, Diamond01 b, Fan00 c);
public class Top01(Diamond02 a, Diamond03 b);
public class Top02(Diamond04 a, Fan05 b, Chain10 c);
public class Top03(Diamond05 a, Diamond06 b, Fan01 c, Leaf00 d);
public class Top04(Diamond07 a, Diamond08 b);
public class Top05(Diamond09 a, Fan10 b, Chain15 c);
public class Top06(Diamond10 a, Diamond11 b, Fan02 c);
public class Top07(Diamond12 a, Diamond13 b);
public class Top08(Diamond14 a, Fan15 b, Chain19 c, Leaf10 d);
public class Top09(Diamond15 a, Diamond16 b);
public class Top10(Diamond17 a, Fan18 b, Chain05 c);
public class Top11(Diamond18 a, Diamond19 b, Fan03 c);
public class Top12(Top00 a, Top01 b);
public class Top13(Top02 a, Top03 b, Diamond00 c);
public class Top14(Top04 a, Top05 b);
public class Top15(Top06 a, Top07 b, Fan19 c);
public class Top16(Top08 a, Top09 b);
public class Top17(Top10 a, Top11 b, Chain00 c);
public class Top18(Top12 a, Top13 b);
public class Top19(Top14 a, Top15 b, Top16 c, Top17 d);
