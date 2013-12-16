using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;

namespace SoftMills.ComplexValidation.Tests
{
	public class TestableValidIfAttribute : ValidIfAttribute
	{
		public TestableValidIfAttribute(string json) : base(json) { }

		public ValidationResult IsValidTest(object value, ValidationContext validationContext)
		{
			return IsValid(value, validationContext);
		}
	}

	[TestClass]
	public class ValidIfAttributeTests
	{
		private static readonly DateTimeOffset now = DateTimeOffset.Now;

		private static readonly object testObject = new
			{
				Zero = 0,
				One = 1,
				Two = 2,
				Empty = "",
				Ex = "x",
				List = new[] {1,2,3},
				Null = (string)null,
				Obj = new
					{
						Three = 3.0,
						Four = 4.0,
						Sub = new
							{
								DtToday = DateTime.Today,
								DtoNow = now,
								Y2K = new DateTime(2000, 1, 1),
							}
					},
				WhichAddress = "SantaClaus",
				SantaClaus = new
					{
						Street = "North Pole",
						Country = 42,
						Postal = "H0H0H0"
					},
				PrimeMinister = new
					{
						Street = "24 Sussex Drive",
						City = "Ottawa",
						Region = "ON",
						Country = 42,
						Postal = "K1M 1M4",
					},
				President = new
					{
						Street = "1600 Pennsylvania Avenue",
						City = "Washington",
						Region = "DC",
						Country = 666,
						Postal = "20500",
					},
				CanadaId = 42,
				UsaId = 666,
			};

		private static readonly ValidationContext testContext = new ValidationContext(testObject);

		[TestMethod]
		public void Required()
		{
			Invalid(null, "''");
			Invalid("", "''");
			Valid("1", "''");

			Invalid(null, "['present']");
			Invalid("", "['present']");
			Valid("1", "['present']");

			Invalid(null, "['present','']");
			Invalid("", "['present','']");
			Valid("1", "['present','']");

			Invalid(1, "['present','Empty']");
			Invalid(1, "['present','Null']");
			Valid(1, "['present','One']");
			Valid(1, "['present','Two']");

			Valid(null, "['absent']");
			Valid("", "['absent']");
			Invalid("1", "['absent']");

			Valid(null, "['absent','']");
			Valid("", "['absent','']");
			Invalid("1", "['absent','']");

			Valid(1, "['absent','Empty']");
			Valid(1, "['absent','Null']");
			Invalid(1, "['absent','One']");
			Invalid(1, "['absent','Two']");
		}

		[TestMethod]
		public void Equal()
		{
			Valid(1, "['eq','Null']");

			Valid(1, "['eq','One']");
			Invalid(1, "['eq','Two']");
			Valid(2, "['eq','Two']");
			Invalid(3, "['eq','Two']");

			Valid(1, "['eq',1]");
			Invalid(1, "['eq',2]");

			Valid(DateTime.Today, "['eq','Obj.Sub.DtToday']");
			Valid(now, "['eq','Obj.Sub.DtoNow']");
			Invalid(DateTime.Today, "['eq','Obj.Sub.DtoNow']");
		}

		[TestMethod]
		public void NotEqual()
		{
			Valid(1, "['lt','Two']");
			Invalid(2, "['lt','Two']");
			Invalid(3, "['lt','Two']");

			Valid(1, "['lte','Two']");
			Valid(2, "['lte','Two']");
			Invalid(3, "['lte','Two']");

			Invalid(1, "['gt','Two']");
			Invalid(2, "['gt','Two']");
			Valid(3, "['gt','Two']");

			Invalid(1, "['gte','Two']");
			Valid(2, "['gte','Two']");
			Valid(3, "['gte','Two']");
		}

		[TestMethod]
		public void Strings()
		{
			Valid(null, "['eq',['concat',['str','a'],['str','.'],['str','b']],['str','a.b']]");
			Valid("North Pole", "['eq',['value',['concat','WhichAddress',['str','.Street']]]]");
			Valid("Joe's", @"['eq',['str','Joe\u0027s']]");
			Valid("Say, \"Hi!\"", @"['eq',['str','Say, \""Hi!\""']]");
			Valid("Say, \"Hi!\"", "['eq',['str','Say, \\\"Hi!\\\"']]");
		}

		[TestMethod]
		public void RegularExpressions()
		{
			Valid("H0H0H0", "['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]");
			Valid("K1M 1M4", "['if',['eq','PrimeMinister.Country','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
				"['eq','PrimeMinister.Country','UsaId'],['regex','[0-9]{5}'],true]");
			Valid("20500", "['if',['eq','President.Country','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
				"['eq','President.Country','UsaId'],['regex',['str','[0-9]{5}']],true]");
			Invalid("205000", "['if',['eq','President.Country','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
				"['eq','President.Country','UsaId'],['regex',['str','[0-9]{5}']],true]");
			Valid("20500", "['with','President',['if',['eq','Country',42],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
				"['eq','Country',666],['regex',['str','[0-9]{5}']],true]]");
			Invalid("205000", "['with','President',['if',['eq','Country',42],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
				"['eq','Country',666],['regex',['str','[0-9]{5}']],true]]");
		}

		[TestMethod]
		public void Complex()
		{
			Valid(666, "['with','President',['eq','Country']]");
			Valid(3, "['or',['and',['gte','One'],['lte','Obj.Three'],['eq','Obj.Three']],'Empty',['lt','Obj.Four']]");
			Valid(2, "['eq',['count','One','Two','Empty','Null']]");
			Valid(0, "['eq',['coalesce','Empty','Null','Zero','One','Two']]");
			Valid(false, "['eq',['and',['not',false],['not',true],true]]");
		}

		[TestMethod]
		public void Lists() {
			Valid(2, "['in','List']");
			Valid(2, "['in',2]");
			Valid(5, "['in','','List',5]");
			Invalid(6, "['in','','List',5]");
			Valid(new[] { 1, 2 }, "['contains','One']");
			Valid(new[] { 1, 2 }, "['or',['contains',3],['contains','One']]");
			Valid(new[] { 1, 2 }, "['or',['contains',2],['contains','Zero']]");
			Invalid(new[] { 1, 3 }, "['or',['contains',2],['contains','Zero']]");
			Valid(null, "['contains','List',2]");
			Valid(null, "['contains','List','Two',2,2]");
			Invalid(null, "['contains','List','Two',2,4]");
			Valid(null, "['contains','List','Two']");
			Valid(3, "['eq',['len','List']]");
		}

		private static void Invalid(object value, string rule)
		{
			Assert.AreNotEqual(ValidationResult.Success, new TestableValidIfAttribute(rule).IsValidTest(value, testContext));
		}

		private static void Valid(object value, string rule)
		{
			Assert.AreEqual(ValidationResult.Success, new TestableValidIfAttribute(rule).IsValidTest(value, testContext));
		}
	}
}