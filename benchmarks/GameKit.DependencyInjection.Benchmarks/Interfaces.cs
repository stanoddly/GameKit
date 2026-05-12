#pragma warning disable CS9113
namespace GameKit.DependencyInjection.Benchmarks;

// Interfaces for alias benchmarking
public interface ILeaf00;
public interface IFan00;
public interface IDiamond00;
public interface ITop00;

// Implementations that also implement interfaces
public class LeafWithInterface00 : ILeaf00;
public class FanWithInterface00(LeafWithInterface00 a, Leaf01 b) : IFan00;
public class DiamondWithInterface00(FanWithInterface00 a, Chain00 b) : IDiamond00;
public class TopWithInterface00(DiamondWithInterface00 a, Fan05 b) : ITop00;
