using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlotMachine.App.Game.Configuration;

namespace SlotMachine.App.Game.Money
{
    /// <summary>
    /// Holds the balance and stake amounts.
    /// </summary>
    public sealed class Wallet : IWallet, IWalletConfiguration
    {
        private readonly object _syncLock;
        private readonly ILogger _logger;
        private readonly AllOptions _options;
        private int _coefficientPrecisionValue;
        private decimal _maxStakeDivisionArgument;
        private WalletState _state;

        public Wallet(ILogger<Wallet> logger, IOptions<AllOptions> options)
        {
            _syncLock = new object();
            _logger = logger;
            _options = options.Value;
            _state = WalletState.Empty;
        }

        public void Prepare()
        {
            _coefficientPrecisionValue = Util.CalculatePrecisionValue(_options.General.CoefficientPrecision);
            _maxStakeDivisionArgument = CalculateMaxStakeDivisionArgument();
        }

        /// <summary>
        /// We have to make sure balance doesn't exceed max amount when making a stake.
        /// In order to do so we have to:
        ///   divide the difference from max amount minus current balance
        ///   by the max combined coefficient - 1 divided by the coefficient precision value
        ///     subtracting 1 from the max combined coefficient accounts for the staked amount which is lost
        ///
        /// Max combined coefficient is calculated as follows:
        ///   multiplication of highest of all symbol coefficients
        ///   with spin row count
        ///   with spin symbol count
        /// </summary>
        private decimal CalculateMaxStakeDivisionArgument()
        {
            int highestSymbolCoefficient = _options.Symbols.Select(s => s.Coefficient).Max();
            int maxCombinedCoefficient = _options.Spin.RowCount * _options.Spin.SymbolCount * highestSymbolCoefficient;
            if (maxCombinedCoefficient <= _coefficientPrecisionValue)
            {
                throw new InvalidOperationException("Max possible profit for a spin needs to be greater than the stake amount.");
            }

            decimal divisionArgument = (maxCombinedCoefficient - _coefficientPrecisionValue) / (decimal)_coefficientPrecisionValue;
            _logger.LogInformation("Calculated max stake division argument: {0}.", divisionArgument);
            return divisionArgument;
        }

        public Amount Balance => _state.Balance;
        public Amount Stake => _state.Stake;
        public string Currency => _options.General.Currency;

        public void Deposit(Amount deposit)
        {
            lock (_syncLock)
            {
                if (_state.Balance > Amount.Zero)
                    throw new InvalidOperationException("Cannot deposit while balance is not zero.");
                if (_state.Stake > Amount.Zero)
                    throw new InvalidOperationException("Cannot deposit while there are money at stake.");

                LogState("Before deposit");
                _state = new WalletState(balance: deposit, stake: Amount.Zero);
                LogState("After deposit");
            }
        }

        public Amount Withdraw()
        {
            lock (_syncLock)
            {
                if (_state.Stake > Amount.Zero)
                    throw new InvalidOperationException("Cannot withdraw while there are money at stake.");

                Amount result = _state.Balance;
                LogState("Before withdraw");
                _state = WalletState.Empty;
                LogState("After withdraw");

                return result;
            }
        }

        public StakeResult TryStake(Amount stake)
        {
            lock (_syncLock)
            {
                if (_state.Stake > Amount.Zero)
                    throw new InvalidOperationException("Cannot stake again while there are already money at stake.");
                if (stake == Amount.Zero)
                    return new StakeResult {InvalidStake = true};
                if (stake > _state.Balance)
                    return new StakeResult {InsufficientBalance = true};

                Amount maxStake = (Amount.Max - _state.Balance) / _maxStakeDivisionArgument;
                if (stake > maxStake)
                {
                    return new StakeResult {MoreThanMaxStake = true, MaxStake = maxStake};
                }

                LogState("Before stake");
                _state = new WalletState(balance: _state.Balance - stake, stake: stake);
                LogState("After stake");
                return new StakeResult {IsSuccess = true};
            }
        }

        public Amount EndStake(int coefficient)
        {
            lock (_syncLock)
            {
                if (_state.Stake == Amount.Zero)
                    throw new InvalidOperationException("Cannot end stake while there are no money at stake.");

                Amount profit = _state.Stake * (coefficient / (decimal) _coefficientPrecisionValue);
                LogState("Before end stake");
                _state = new WalletState(balance: _state.Balance + profit, stake: Amount.Zero);
                LogState("After end stake");

                return profit;
            }
        }

        private void LogState(string reason)
        {
            _logger.LogInformation("{0}: balance = {1} {2}, stake = {3} {4}.", reason, Balance, Currency, Stake, Currency);
        }

        private sealed class WalletState
        {
            public WalletState(Amount balance, Amount stake)
            {
                Balance = balance;
                Stake = stake;
            }

            public Amount Balance { get; }
            public Amount Stake { get; }

            public static WalletState Empty => new WalletState(balance: Amount.Zero, stake: Amount.Zero);
        }
    }
}
