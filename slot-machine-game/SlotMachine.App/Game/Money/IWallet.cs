namespace SlotMachine.App.Game.Money
{
    public interface IWallet
    {
        Amount Balance { get; }

        Amount Stake { get; }

        string Currency { get; }

        void Deposit(Amount deposit);

        Amount Withdraw();

        StakeResult TryStake(Amount stake);

        Amount EndStake(int coefficient);
    }

    public sealed class StakeResult
    {
        public bool IsSuccess { get; set; }
        public bool InvalidStake { get; set; }
        public bool InsufficientBalance { get; set; }
        public bool MoreThanMaxStake { get; set; }
        public Amount MaxStake { get; set; }
    }

    public interface IWalletConfiguration
    {
        void Prepare();
    }
}