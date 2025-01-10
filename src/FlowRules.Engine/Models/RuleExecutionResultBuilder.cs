using System;

namespace FlowRules.Engine.Models
{
    /// <summary>
    /// Constructs a <see cref="RuleExecutionResult"/>.
    /// </summary>
    /// <param name="id">The id of the rule.</param>
    /// <param name="name">The name of the rule.</param>
    /// <param name="description">An optional description for the rule.</param>
    internal class RuleExecutionResultBuilder(string id, string name, string? description)
    {
        private bool _passed;
        private string? _message;
        private Exception? _exception;
        private TimeSpan _elapsedTime;

        /// <summary>
        /// Indicates the <see cref="RuleExecutionResult"/> should be constructed in a successful state.
        /// </summary>
        /// <returns>The instance of the <see cref="RuleExecutionResultBuilder"/>.</returns>
        public RuleExecutionResultBuilder WithSuccess()
        {
            _passed = true;
            return this;
        }

        /// <summary>
        /// Indicates the <see cref="RuleExecutionResult"/> should be constructed in a failure state.
        /// </summary>
        /// <param name="message">Adds a failure / exception message to the <see cref="RuleExecutionResultBuilder"/>.</param>
        /// <returns>The instance of the <see cref="RuleExecutionResultBuilder"/>.</returns>
        public RuleExecutionResultBuilder WithFailure(string? message)
        {
            _message = message;
            _passed = false;
            return this;
        }

        /// <summary>
        /// Adds an exception to the rule result.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The instance of the <see cref="RuleExecutionResultBuilder"/>.</returns>
        public RuleExecutionResultBuilder WithException(Exception ex)
        {
            _exception = ex;
            _message = ex.Message;
            return this;
        }

        /// <summary>
        /// Adds the elapsed rule execution time to the results.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        /// <returns>The instance of the <see cref="RuleExecutionResultBuilder"/>.</returns>
        public RuleExecutionResultBuilder WithTime(TimeSpan elapsedTime)
        {
            _elapsedTime = elapsedTime;
            return this;
        }

        /// <summary>
        ///  Creates the <see cref="RuleExecutionResult"/>.
        /// </summary>
        /// <returns>An instance of the <see cref="RuleExecutionResult"/>.</returns>
        public RuleExecutionResult ToRuleExecutionResult()
        {
            return new RuleExecutionResult(
                id,
                name,
                _passed,
                _elapsedTime,
                description,
                _message,
                _exception);
        }
    }
}
