using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftMills.ComplexValidation.Mvc.Tests.Models
{
	public class HomeIndexModel
	{
		public class Location
		{
			public string Street { get; set; }
			public string City { get; set; }
			public string Region { get; set; }
			public int Country { get; set; }
			public string Postal { get; set; }
		}

		public class SubModel
		{
			public class SubSubModel
			{
				public DateTime DtToday { get; set; }
				public DateTimeOffset DtoNow { get; set; }
				public DateTime Y2K { get; set; }
			}

			public double Three { get; set; }
			public double Four { get; set; }
			public SubSubModel Sub { get; set; }
		}

		public int Zero { get; set; }
		public int One { get; set; }
		public int Two { get; set; }
		public string Empty { get; set; }
		public string Ex { get; set; }
		public string Null { get; set; }
		public SubModel Obj { get; set; }
		public string WhichAddress { get; set; }
		public Location SantaClaus { get; set; }
		public Location PrimeMinister { get; set; }
		public Location President { get; set; }
		public int CanadaId { get; set; }
		public int UsaId { get; set; }

		public IEnumerable<int> NumberSet { get; set; }

		[Display(Name = "This input field")]
		[ValidIf("['eq',['str','a']]", ErrorMessage = "{0} must equal 'a'.")]
		public string Input { get; set; }

		[Display(Name = "Minimum")]
		[Required]
		public int? RangeMin { get; set; }

		[Display(Name = "Current")]
		[Required]
		[GreaterThanOrEqualTo("RangeMin")]
		[LessThanOrEqualTo("RangeMax")]
		public int? RangeCurrent { get; set; }

		[Display(Name = "Maximum")]
		[Required]
		public int? RangeMax { get; set; }

		[ValidIf("''")]
		public string InvalidReqPresentNull1 { get; set; }

		[ValidIf("''")]
		public string InvalidReqPresentEmpty1 { get; set; }

		[ValidIf("''")]
		public string ValidReqPresentOne1 { get; set; }

		[ValidIf("['present']")]
		public string InvalidReqPresentNull2 { get; set; }

		[ValidIf("['present']")]
		public string InvalidReqPresentEmpty2 { get; set; }

		[ValidIf("['present']")]
		public string ValidReqPresentOne2 { get; set; }

		[ValidIf("['present','']")]
		public string InvalidReqPresentNull3 { get; set; }

		[ValidIf("['present','']")]
		public string InvalidReqPresentEmpty3 { get; set; }

		[ValidIf("['present','']")]
		public string ValidReqPresentOne3 { get; set; }

		[ValidIf("['present','Empty']")]
		public string InvalidReqPresentEmpty { get; set; }

		[ValidIf("['present','Null']")]
		public string InvalidReqPresentNull { get; set; }

		[ValidIf("['present','One']")]
		public string InvalidReqPresentOne { get; set; }

		[ValidIf("['present','Two']")]
		public string InvalidReqPresentTwo { get; set; }

		[ValidIf("['absent']")]
		public string ValidReqAbsentNull2 { get; set; }

		[ValidIf("['absent']")]
		public string ValidReqAbsentEmpty2 { get; set; }

		[ValidIf("['absent']")]
		public string InvalidReqAbsentOne2 { get; set; }

		[ValidIf("['absent','']")]
		public string ValidReqAbsentNull3 { get; set; }

		[ValidIf("['absent','']")]
		public string ValidReqAbsentEmpty3 { get; set; }

		[ValidIf("['absent','']")]
		public string InvalidReqAbsentOne3 { get; set; }

		[ValidIf("['eq',['concat',['str','a'],['str','.'],['str','b']],['str','a.b']]")]
		public string ValidStringConcat { get; set; }

		[ValidIf("['eq',['value',['concat','WhichAddress',['str','.Street']]]]")]
		public string ValidStringConcatWithRef { get; set; }

		[ValidIf(@"['eq',['str','Joe\u0027s']]")]
		public string ValidStringSingleQuoteEncoding { get; set; }

		[ValidIf(@"['eq',['str','Say, \""Hi!\""']]")]
		public string ValidStringDoubleQuoteEncoding1 { get; set; }

		[ValidIf("['eq',['str','Say, \\\"Hi!\\\"']]")]
		public string ValidStringDoubleQuoteEncoding2 { get; set; }

		[ValidIf("['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]")]
		public string ValidRegexPostal { get; set; }

		[ValidIf("['if',['eq','PrimeMinister.Country','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
			"['eq','PrimeMinister.Country','UsaId'],['regex',['str','[0-9]{5}']],true]")]
		public string ValidRegexPostalCan { get; set; }

		[ValidIf("['if',['eq','President.Country','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
			"['eq','President.Country','UsaId'],['regex',['str','[0-9]{5}']],true]")]
		public string ValidRegexPostalUs { get; set; }

		[ValidIf("['if',['eq','President.Country','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
			"['eq','President.Country','UsaId'],['regex',['str','[0-9]{5}']],true]")]
		public string InvalidRegexPostalUs { get; set; }

		[ValidIf("['with',['str','President'],['if',['eq','Country',42],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
			"['eq','Country',666],['regex',['str','[0-9]{5}']],true]]")]
		public string ValidRegexPostalUsWith { get; set; }

		[ValidIf("['with',['str','President'],['if',['eq','Country',42],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]," +
			"['eq','Country',666],['regex',['str','[0-9]{5}']],true]]")]
		public string InvalidRegexPostalUsWith { get; set; }

		[ValidIf("['with',['str','President'],['eq','Country']]")]
		public int ValidComplexWith { get; set; }

		[ValidIf("['or',['and',['gte','One'],['lte','Obj.Three'],['eq','Obj.Three']],'Empty',['lt','Obj.Four']]")]
		public int ValidComplexBool { get; set; }

		[ValidIf("['eq',['count','One','Two','Empty','Null']]")]
		public int ValidComplexCount { get; set; }

		[ValidIf("['eq',['coalesce','Empty','Null','Zero','One','Two']]")]
		public int ValidComplexCoalesce { get; set; }

		[ValidIf("['eq',['and',['not',false],['not',true],true]]")]
		public bool ValidComplexBool2 { get; set; }

		[ValidIf("['eq',['delay',1000,5]]")]
		public int ValidDelayedTest { get; set; }

		public IEnumerable<int> ValidSet { get; set; }

		public static HomeIndexModel Default
		{
			get
			{
				return new HomeIndexModel
				{
					Zero = 0,
					One = 1,
					Two = 2,
					Empty = "",
					Ex = "x",
					Null = null,
					Obj = new SubModel
					{
						Three = 3.0,
						Four = 4.0,
						Sub = new SubModel.SubSubModel
						{
							DtToday = DateTime.Today,
							DtoNow = DateTimeOffset.Now,
							Y2K = new DateTime(2000, 1, 1),
						}
					},
					WhichAddress = "SantaClaus",
					SantaClaus = new Location
						{
							Street = "North Pole",
							Country = 42,
							Postal = "H0H0H0",
						},
					PrimeMinister = new Location
						{
							Street = "24 Sussex Drive",
							City = "Ottawa",
							Region = "ON",
							Country = 42,
							Postal = "K1M 1M4",
						},
					President = new Location
						{
							Street = "1600 Pennsylvania Avenue",
							City = "Washington",
							Region = "DC",
							Country = 666,
							Postal = "20500",
						},
					CanadaId = 42,
					UsaId = 666,

					InvalidReqPresentNull1 = null,
					InvalidReqPresentEmpty1 = "",
					ValidReqPresentOne1 = "1",
					InvalidReqPresentNull2 = null,
					InvalidReqPresentEmpty2 = "",
					ValidReqPresentOne2 = "1",
					InvalidReqPresentNull3 = null,
					InvalidReqPresentEmpty3 = "",
					ValidReqPresentOne3 = "1",
					ValidReqAbsentNull2 = null,
					ValidReqAbsentEmpty2 = "",
					InvalidReqAbsentOne2 = "1",
					ValidReqAbsentNull3 = null,
					ValidReqAbsentEmpty3 = "",
					InvalidReqAbsentOne3 = "1",

					ValidStringConcatWithRef = "North Pole",
					ValidStringSingleQuoteEncoding = "Joe's",
					ValidStringDoubleQuoteEncoding1 = "Say, \"Hi!\"",
					ValidStringDoubleQuoteEncoding2 = "Say, \"Hi!\"",

					ValidRegexPostal = "H0H0H0",
					ValidRegexPostalCan = "K1M 1M4",
					ValidRegexPostalUs = "20500",
					InvalidRegexPostalUs = "205000",
					ValidRegexPostalUsWith = "20500",
					InvalidRegexPostalUsWith = "205000",

					ValidComplexWith = 666,
					ValidComplexBool = 3,
					ValidComplexCount = 2,
					ValidComplexCoalesce = 0,
					ValidComplexBool2 = false,

					ValidDelayedTest = 5,
				};
			}
		}
	}
}