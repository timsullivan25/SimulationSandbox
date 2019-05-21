using System;
using System.Collections.Generic;
using System.Linq;
using Simulations.Exceptions;
using System.Collections;
using Nager.Date;

namespace Simulations
{
    public class BinomialTreeNode : IEnumerable<BinomialTreeNode>
    {
        // private backing variables
        private readonly List<BinomialTreeNode> _parents = new List<BinomialTreeNode>();
        private readonly List<BinomialTreeNode> _children = new List<BinomialTreeNode>();
        private double _cumulativeProbability = 0.0d;


        // properties
        public string ID { get; private set; }
        public double Value { get; private set; }
        public List<BinomialTreeNode> Parents
        {
            get
            {
                return _parents;
            }
        }
        public int NumberOfParents
        {
            get
            {
                return this._parents.Count;
            }
        }
        public bool HasParent
        {
            get
            {
                return this.NumberOfParents > 0;
            }
        }
        public List<BinomialTreeNode> Children
        {
            get
            {
                return _children;
            }
        }
        public int NumberOfChildren
        {
            get
            {
                return this._children.Count;
            }
        }
        public bool HasChildren
        {
            get
            {
                return this.NumberOfChildren > 0;
            }
        }
        public int Level
        {
            get
            {
                int level = 0;
                BinomialTreeNode node = this;

                while (true)
                {
                    if (node.HasParent)
                    {
                        level++;
                        node = node.Parents.First();
                    }
                    else
                    {
                        break;
                    }
                }

                return level;
            }
        }


        // tracking properties
        public BinomialTreeMove Move { get; private set; }
        public double UpProbability { get; private set; }
        public double DownProbability { get; private set; }
        public double CumulativeProbability
        {
            get
            {
                return _cumulativeProbability;
            }
            private set
            {
                _cumulativeProbability = value;
            }
        }
        public double Volatility { get; private set; }


        // constructor
        public BinomialTreeNode(double value, double upProbability, double volatility, BinomialTreeMove moveDirection)
        {
            if (upProbability < 0.0d || upProbability > 1.0d)
                throw new InvalidProbabilityException($"Up probability of {upProbability} is not between 0 and 100%.");
            
            this.Value = value;
            this.Move = moveDirection;
            this.UpProbability = upProbability;
            this.DownProbability = 1.00d - upProbability;
            this.Volatility = volatility;

            // handle non-standard move types
            if (moveDirection == BinomialTreeMove.Root)
            {
                this.CumulativeProbability = 1.0d;
            }
            else if (moveDirection == BinomialTreeMove.Converge)
            {
                throw new InvalidMoveException("Nodes cannot be constructed with the Converge move type. That type is reserved for internal use when a second parent is added to a node.");
            }
        }


        // methods
        public void AddChild(BinomialTreeNode newChildNode)
        {
            if (newChildNode._parents.Contains(this) == false)
                newChildNode._parents.Add(this);

            if (this._children.Contains(newChildNode) == false)
                this._children.Add(newChildNode);

            newChildNode.CalculateCumulativeProbability();
        }
        public void RemoveChild(BinomialTreeNode childNode)
        {
            childNode._parents.Remove(this);
            this._children.Remove(childNode);
        }
        public void AddParent(BinomialTreeNode newParentNode)
        {
            if (this._parents.Contains(newParentNode) == false)
                this._parents.Add(newParentNode);

            if (newParentNode._children.Contains(this) == false)
                newParentNode._children.Add(this);

            this.CalculateCumulativeProbability();
        }
        public void RemoveParent(BinomialTreeNode parentNode)
        {
            this._parents.Remove(parentNode);
            parentNode._children.Remove(this);
            this.CalculateCumulativeProbability();
        }


        // internal methods
        internal void CalculateCumulativeProbability()
        {
            // handle all cumulative probability calculations here to avoid recursion later
            if (this.NumberOfParents == 1)
            {
                if (this.Move == BinomialTreeMove.Up)
                {
                    this.CumulativeProbability = this.Parents[0].CumulativeProbability * this.Parents[0].UpProbability;
                }
                else if (this.Move == BinomialTreeMove.Down)
                {
                    this.CumulativeProbability = this.Parents[0].CumulativeProbability * this.Parents[0].DownProbability;
                }
                else
                {
                    throw new InvalidMoveException($"It should not be possible to be adding the first parent when the node move type is {this.Move}.");
                }
            }
            else if (this.NumberOfParents == 2)
            {
                // detemrine which node was reponsible for the first move, second (this) move must be opposite of the first
                if (this.Move == BinomialTreeMove.Up)
                {
                    this.CumulativeProbability += this.Parents[1].CumulativeProbability * this.Parents[1].DownProbability;
                }
                else if (this.Move == BinomialTreeMove.Down)
                {
                    this.CumulativeProbability += this.Parents[1].CumulativeProbability * this.Parents[1].UpProbability;
                }
                else
                {
                    throw new InvalidMoveException($"{this.Move} is not a valid first move when attempting to add a second parent.");
                }

                // nodes are now converged
                this.Move = BinomialTreeMove.Converge;
            }
            else
            {
                throw new TooManyParentsException($"It should not be possible for a node to have {this.NumberOfParents} parents. Convergence of nodes should limit number of parents to a maximum of two nodes when each move is calculated using a linear function.");
            }
        }


        // enumeration interface
        public IEnumerator<BinomialTreeNode> GetEnumerator()
        {
            return this._children.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public struct ConfidenceInterval
    {
        public ConfidenceLevel ConfidenceLevel { get; private set; }
        public double ZScore
        {
            get
            {
                return ConfidenceLevel.ZScore();
            }
        }
        public double LowerBound { get; private set; }
        public double UpperBound { get; private set; }
        public double Range
        {
            get
            {
                return UpperBound - LowerBound;
            }
        }

        public ConfidenceInterval(ConfidenceLevel confidenceLevel, double lowerBound, double upperBound)
        {
            this.ConfidenceLevel = confidenceLevel;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
        }
    }

    public class OptionContract
    {
        // properties
        public OptionExecutionRules ExecutionRules { get; set; }
        public OptionContractType Type { get; set; }
        public int DayCountConvention { get; set; }
        public double StrikePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public double RiskFreeRate { get; set; }


        // valuation dates
        public List<DateTime> TradingDays { get; private set; }
        private void ComputeTradingDays()
        {
            List<DateTime> tradingDays = new List<DateTime>();
            DateTime currentDate = this.PurchaseDate.AddDays(1);

            while (currentDate <= ExpirationDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                    currentDate.DayOfWeek != DayOfWeek.Sunday &&
                    !DateSystem.IsPublicHoliday(currentDate, CountryCode.US))
                {
                    tradingDays.Add(currentDate);
                }

                currentDate = currentDate.AddDays(1);
            }

            this.TradingDays = tradingDays;
        }


        // summary statistics
        public int DaysRemaining
        {
            get
            {
                return Math.Max(0, (ExpirationDate - DateTime.Today).Days);
            }
        }
        public int TradingDaysRemaining
        {
            get
            {
                return this.TradingDays.Count;
            }
        }
        /* Delta
           Gamma
           Vega
           Theta
           Roe   */


        // constructors -- should add country code argument?
        public OptionContract(OptionContractType type, double strikePrice, double riskFreeRate, DateTime expirationDate, OptionExecutionRules executionRules = OptionExecutionRules.American, int dayCountConvention = 360, DateTime? purchaseDate = null)
        {
            this.ExecutionRules = executionRules;
            this.Type = type;
            this.DayCountConvention = dayCountConvention;
            this.StrikePrice = strikePrice;
            this.PurchaseDate = purchaseDate.HasValue ? DateTime.Today : purchaseDate.Value;
            this.ExpirationDate = expirationDate;
            this.RiskFreeRate = riskFreeRate;

            ComputeTradingDays();
        }

        public OptionContract(OptionContractType type, double strikePrice, double riskFreeRate, int tradingDaysUntilExpiration, OptionExecutionRules executionRules = OptionExecutionRules.American, int dayCountConvention = 360)
        {
            this.ExecutionRules = executionRules;
            this.Type = type;
            this.DayCountConvention = dayCountConvention;
            this.StrikePrice = strikePrice;
            this.PurchaseDate = DateTime.Today;
            this.RiskFreeRate = riskFreeRate;

            // need to walk forward to ensure there are enough trading days between now and expiration
            DateTime currentDate = DateTime.Today.AddDays(1);
            List<DateTime> tradingDays = new List<DateTime>();
            int totalNumberOfDays = 0;

            while (tradingDays.Count < tradingDaysUntilExpiration)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                    currentDate.DayOfWeek != DayOfWeek.Sunday &&
                    !DateSystem.IsPublicHoliday(currentDate, CountryCode.US))
                {
                    tradingDays.Add(currentDate);
                }

                totalNumberOfDays++;
                currentDate = currentDate.AddDays(1);
            }

            this.ExpirationDate = PurchaseDate.AddDays(totalNumberOfDays);
            this.TradingDays = tradingDays;
        }
    }

    public class OptionPricingTreeNode : IEnumerable
    {
        // private backing variables
        internal BinomialTreeMove _originalMove;
        private readonly List<OptionPricingTreeNode> _parents = new List<OptionPricingTreeNode>();
        private readonly List<OptionPricingTreeNode> _children = new List<OptionPricingTreeNode>();
        private double _cumulativeProbability = 0.0d;


        // properties
        public DateTime? TradeDate
        {
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    if (this.HasChildren)
                    {
                        return this.OptionContract.TradingDays[this.Level * DaysBetweenPeriods];
                    }
                    else
                    {
                        return this.OptionContract.TradingDays.Last();
                    }
                }
            }
        }
        public int DaysBetweenPeriods { get; private set; }
        public double UnderlyingPrice { get; private set; }
        public List<OptionPricingTreeNode> Parents
        {
            get
            {
                return _parents;
            }
        }
        public int NumberOfParents
        {
            get
            {
                return this._parents.Count;
            }
        }
        public bool HasParent
        {
            get
            {
                return this.NumberOfParents > 0;
            }
        }
        public List<OptionPricingTreeNode> Children
        {
            get
            {
                return _children;
            }
        }
        public int NumberOfChildren
        {
            get
            {
                return this._children.Count;
            }
        }
        public bool HasChildren
        {
            get
            {
                return this.NumberOfChildren > 0;
            }
        }
        public int Level
        {
            get
            {
                int level = 0;
                OptionPricingTreeNode node = this;

                while (true)
                {
                    if (node.HasParent)
                    {
                        level++;
                        node = node.Parents.First();
                    }
                    else
                    {
                        break;
                    }
                }

                return level;
            }
        }


        // option specific properties
        public OptionContract OptionContract { get; private set; }
        public double UpFactor { get; private set; }
        public double DownFactor { get; private set; }
        public double? IntrinsicOptionPrice
        {
            get
            {
                if (OptionContract == null)
                {
                    return null;
                }
                else
                {
                    if (OptionContract.Type == OptionContractType.Call)
                    {
                        return Math.Max(0, this.UnderlyingPrice - OptionContract.StrikePrice);
                    }
                    else if (OptionContract.Type == OptionContractType.Put)
                    {
                        return Math.Max(0, OptionContract.StrikePrice - this.UnderlyingPrice);
                    }
                    else
                    {
                        throw new InvalidOptionTypeException($"{OptionContract.Type} is not a valid option contract type.");
                    }
                }
            }
        }      
        public double? PureBinomialValue
        {
            // this should cause full out recursive calculation, as is necessary to compute option price at t0
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    if (this.HasChildren == false)
                    {
                        return this.IntrinsicOptionPrice;
                    }
                    else
                    {
                        // first child is always up, second is always down
                        double binomialUpValue = this.UpProbability * (double)this.Children[0].BinomialValue;
                        double binomialDownValue = this.DownProbability * (double)this.Children[1].BinomialValue;
                        int daysBetweenTrades = this.HasParent == false ? 0 : ((DateTime)this.TradeDate - (DateTime)this.Parents[0].TradeDate).Days;
                        return (binomialUpValue + binomialDownValue) * Math.Pow(Math.E, -1 * this.OptionContract.RiskFreeRate * daysBetweenTrades / this.OptionContract.DayCountConvention);
                    }
                }
            }
        }
        public double? BinomialValue
        {
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    if (this.OptionContract.ExecutionRules == OptionExecutionRules.American)
                    {
                        return Math.Max((double)this.IntrinsicOptionPrice, (double)this.PureBinomialValue);
                    }
                    else if (this.OptionContract.ExecutionRules == OptionExecutionRules.European)
                    {
                        return this.PureBinomialValue;
                    }
                    else
                    {
                        throw new NotImplementedException($"{this.OptionContract.ExecutionRules} execution rules have not yet been implemented.");
                    }
                }
            }
        }


        // tracking properties
        public BinomialTreeMove Move { get; private set; }
        public double UpProbability { get; private set; }
        public double DownProbability { get; private set; }
        public double CumulativeProbability
        {
            get
            {
                return _cumulativeProbability;
            }
            private set
            {
                _cumulativeProbability = value;
            }
        }
        public double Volatility { get; private set; }


        // constructor
        public OptionPricingTreeNode(int daysBetweenPeriods, double underlyingPrice, double upProbability, double volatility, double upFactor, double downFactor, BinomialTreeMove moveDirection, OptionContract optionContract = null)
        {
            if (upProbability < 0.0d || upProbability > 1.0d)
                throw new InvalidProbabilityException($"Up probability of {upProbability} is not between 0 and 100%.");

            this.DaysBetweenPeriods = daysBetweenPeriods;
            this.UnderlyingPrice = underlyingPrice;
            this.UpProbability = upProbability;
            this.DownProbability = 1.0d - upProbability;
            this.Volatility = volatility;
            this.UpFactor = upFactor;
            this.DownFactor = downFactor;
            this._originalMove = moveDirection;
            this.Move = moveDirection;
            this.OptionContract = optionContract;

            // handle non-standard move types
            if (moveDirection == BinomialTreeMove.Root)
            {
                this.CumulativeProbability = 1.0d;
            }
            else if (moveDirection == BinomialTreeMove.Converge)
            {
                throw new InvalidMoveException("Nodes cannot be constructed with the Converge move type. That type is reserved for internal use when a second parent is added to a node.");
            }
        }


        // methods
        public void AddChild(OptionPricingTreeNode newChildNode)
        {
            if (newChildNode._parents.Contains(this) == false)
                newChildNode._parents.Add(this);

            if (this._children.Contains(newChildNode) == false)
                this._children.Add(newChildNode);

            newChildNode.CalculateCumulativeProbability();
        }
        public void RemoveChild(OptionPricingTreeNode childNode)
        {
            childNode._parents.Remove(this);
            this._children.Remove(childNode);
        }
        public void AddParent(OptionPricingTreeNode newParentNode)
        {
            if (this._parents.Contains(newParentNode) == false)
                this._parents.Add(newParentNode);

            if (newParentNode._children.Contains(this) == false)
                newParentNode._children.Add(this);

            this.CalculateCumulativeProbability();
        }
        public void RemoveParent(OptionPricingTreeNode parentNode)
        {
            this._parents.Remove(parentNode);
            parentNode._children.Remove(this);
            this.CalculateCumulativeProbability();
        }


        // internal methods
        internal void CalculateCumulativeProbability()
        {
            // handle all cumulative probability calculations here to avoid recursion later
            if (this.NumberOfParents == 1)
            {
                if (this.Move == BinomialTreeMove.Up)
                {
                    this.CumulativeProbability = this.Parents[0].CumulativeProbability * this.Parents[0].UpProbability;
                }
                else if (this.Move == BinomialTreeMove.Down)
                {
                    this.CumulativeProbability = this.Parents[0].CumulativeProbability * this.Parents[0].DownProbability;
                }
                else
                {
                    throw new InvalidMoveException($"It should not be possible to be adding the first parent when the node move type is {this.Move}.");
                }
            }
            else if (this.NumberOfParents == 2)
            {
                // detemrine which node was reponsible for the first move, second (this) move must be opposite of the first
                if (this.Move == BinomialTreeMove.Up)
                {
                    this.CumulativeProbability += this.Parents[1].CumulativeProbability * this.Parents[1].DownProbability;
                }
                else if (this.Move == BinomialTreeMove.Down)
                {
                    this.CumulativeProbability += this.Parents[1].CumulativeProbability * this.Parents[1].UpProbability;
                }
                else
                {
                    throw new InvalidMoveException($"{this.Move} is not a valid first move when attempting to add a second parent.");
                }

                // nodes are now converged
                this.Move = BinomialTreeMove.Converge;
            }
            else
            {
                throw new TooManyParentsException($"It should not be possible for a node to have {this.NumberOfParents} parents. Convergence of nodes should limit number of parents to a maximum of two nodes when each move is calculated using a linear function.");
            }
        }


        // enumeration interface
        public IEnumerator<OptionPricingTreeNode> GetEnumerator()
        {
            return this._children.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class ParameterConstraint
    {
        public double? LowerBound { get; set; }
        public double? UpperBound { get; set; }        
        public ConstraintViolationResolution ConstraintViolationResolution { get; set; }
        public int MaxResimulations { get; set; }
        public double? DefaultValue { get; set; }

        /// <summary>
        /// A struct containing information on how to apply an optional constraint to some types types of IParameters. Currently Distribution, Simulation, and DependentSimulation parameters will accept a constraint object.
        /// </summary>
        /// <param name="upperBound">Maximimum value the underlying parameter can take before violating the constraint. Provide NULL if there is no upper bound.</param>
        /// <param name="lowerBound">Minimum value the underlying parameter can take before violating the constraint. Provide NULL if there is no lower bound.</param>
        /// <param name="constraintViolationResolution">An enum containing information on how the constraint violation should be handled.</param>
        /// <param name="maxResimulations">If the constraint resolution is set the resimulate, the maximum number of resimulations that can occur before the default value is used.</param>
        /// <param name="defaultValue">The default value to be used for constraint violation resolution when specified or when the maximimum number of resimulations is reached.</param>
        public ParameterConstraint(double? lowerBound, double? upperBound, ConstraintViolationResolution constraintViolationResolution = ConstraintViolationResolution.ClosestBound, int maxResimulations = 10, double? defaultValue = null)
        {
            // validation
            if (upperBound == null && lowerBound == null)
                throw new InvalidConstraintException("A constraint must have at least one bound.");

            if ((constraintViolationResolution == ConstraintViolationResolution.DefaultValue || constraintViolationResolution == ConstraintViolationResolution.Resimulate) && defaultValue == null)
                throw new InvalidConstraintException("A constraint that uses DefaultValue or Resimulate for violation resolution must have a default value.");


            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;           
            this.ConstraintViolationResolution = constraintViolationResolution;
            this.MaxResimulations = maxResimulations;
            this.DefaultValue = defaultValue;
        }
    }

    public struct QualitativeSimulationOutcome
    {
        public string Outcome { get; private set; }
        public int Count { get; private set; }
        public double Probability { get; private set; }

        public QualitativeSimulationOutcome(string outcome, int count, double probability)
        {
            this.Outcome = outcome;
            this.Count = count;
            this.Probability = probability;
        }
    }   
}