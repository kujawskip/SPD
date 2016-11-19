namespace SPD.Engine.Strategies
{
    public interface IStrategy
    {
        bool Decide(Coord opponent);
        void PostProcess(Coord opponent, bool opponentBetrayed);
        void Clear();
        void EndStep();
        IStrategy GetCopy();
        int StrategyCode { get; }
    }
}
