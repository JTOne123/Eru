﻿using Xunit;


namespace Eru.Tests
{
    using static Eru._;

    public class ValidationProperties
    {
        private static Unit Fail()
        {
            Assert.True(false);
            return Unit;
        }


        //[Fact(DisplayName =
        //    "Check will only aggregate failed validations")]
        //public void Test3()
        //{
        //    var person = new Person
        //    {
        //        Age = 11,
        //        Name = ""
        //    };

        //    person
        //        .Check(p => p.Age >= 0, "Must have a valid age")
        //        .Check(p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
        //        .Match(_ => Fail(), error =>
        //        {
        //            Assert.Equal("Must have a name", error);
        //            return Unit;
        //        });
        //    ;
        //}

        [Fact(DisplayName =
            "Check will aggregate all failed validations")]
        public void Test4()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };

            person
                .Check<Person, string>((p => p.Age >= 18, "Must have a valid age"), (p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name"))
                .Match(_ => Fail(), error =>
                {
                    Assert.Equal("Must have a name", error);
                    return Unit;
                });
            ;
        }
    }

    public class Person
    {
        public int Age { get; internal set; }
        public string Name { get; internal set; }
    }
}