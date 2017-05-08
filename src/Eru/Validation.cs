﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru
{
    public static partial class _
    {
        public static Error Error(params string[] message) => new Error(message);
    }

    public class Error
    {
        public Error(params string[] messages)
        {
            Messages = messages;
        }

        public string[] Messages { get; }

        public static implicit operator Error(string message)
            => new Error(message);

        public static implicit operator Error(string[] messages)
            => new Error(messages);

        public static implicit operator string[] (Error error)
            => error.Messages;

    }

    public class ErrorEqualityComparer : EqualityComparer<Error>
    {
        public override bool Equals(Error x, Error y) => x.Messages.SequenceEqual(x.Messages);

        public override int GetHashCode(Error obj) => obj.GetHashCode();
    }

    public static class ValidationExtensions
    {
        private static Func<TValue, Either<TValue, Error>> FailFast<TValue>(
             Func<TValue, Either<TValue, Error>>[] validations) =>
                value =>
                    validations.Aggregate(value.Return<TValue, Error>(), (current, validation) =>
                        current.Bind(_ => validation(value)));

        private static Func<TValue, Either<TValue, Error>> HarvestErrors<TValue>(Func<TValue, Either<TValue, Error>>[] validations)
             =>
                value =>
                {
                    var errors = validations
                        .Select(validate => validate(value))
                        .SelectMany(either => either.Alternative())
                        .ToList();

                    return errors.Count == 0
                        ? value.Return<TValue, Error>()
                        : errors.Aggregate((current, next) => _.Error(current.Messages.Concat(next.Messages).ToArray())).ReturnAlternative<TValue, Error>();
                };


        /// <summary>
        /// Validate and fail as soon as one validation fails and do not aggragate any error messages
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<TValue, Error> CheckQuick<TValue>(this TValue value,
            params Func<TValue, Either<TValue, Error>>[] validations)
           =>
                Check(value, FailFast, validations);

        /// <summary>
        /// Validate and aggregate all validation errors that may occur
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<TValue, Error> Check<TValue>(this TValue value,
            params Func<TValue, Either<TValue, Error>>[] validations) =>
                Check(value, HarvestErrors, validations);

        private static Either<TValue, Error> Check<TValue>(this TValue value,
            Func<Func<TValue, Either<TValue, Error>>[], Func<TValue, Either<TValue, Error>>> reduceValidations,
            params Func<TValue, Either<TValue, Error>>[] validations) =>
                reduceValidations(validations)(value);


        public static Either<TValue, Error> Check<TValue>(this TValue value, params (Predicate<TValue> rule, Error error)[] validations) =>
                    value
                        .Return<TValue, Error>()
                        .Check(validations);

        private static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
            params Func<TValue, Either<TValue, Error>>[] validations) =>
                    either.Bind(value => Check(value, validations));

        public static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
            params (Predicate<TValue> rule, Error error)[] validations) =>
            either.Bind(value =>
                Check(value, validations.
                        Select<(Predicate<TValue> rule, Error error), Func<TValue, Either<TValue, Error>>>(tuple =>
                          v => tuple.rule(v)
                              ? v.Return<TValue, Error>()
                              : tuple.error.ReturnAlternative<TValue, Error>()).ToArray()));

        public static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
            Predicate<TValue> rule, Error error)
                => Check(either, (rule, error));

        public static Either<TValue, Error> Check<TValue>(this TValue value,
            Predicate<TValue> rule, Error error)
            => Check(value, (rule, error));

    }
}
