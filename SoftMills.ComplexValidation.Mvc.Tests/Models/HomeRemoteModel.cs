using SoftMills.ComplexValidation.Mvc.Tests.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftMills.ComplexValidation.Mvc.Tests.Models {

	public class HomeRemoteModel {
		private UrlHelper url;

		public HomeRemoteModel(UrlHelper url) {
			this.url = url;
		}

		[Display(Name = "Number of Toppings")]
		[ValidIf("['remote','ToppingCountUrl']", ErrorMessage = "Sorry, that number of toppings is not available.")]
		public int ToppingCount { get; set; }
		public string ToppingCountUrl { get { return url.HomeToppingCountCheck(); } }
		public IEnumerable<SelectListItem> ToppingCountSelectList {
			get {
				return Enumerable.Range(1, ToppingsMultiSelectList.Count()).Select(num => new SelectListItem {
					Value = num.ToString(), Text = num.ToString(),
				});
			}
		}

		[Display(Name = "Toppings")]
		[ValidIf("['eq',['len'],'ToppingCount']", ErrorMessage = "Please pick the toppings you paid for.")]
		[ValidIf2("['or',['not',['contains',5]],['contains',1]]", ErrorMessage = "You can't have Extra Ham without choosing Ham as well.")]
		[ValidIf3("['remote','ToppingsUrl']", ErrorMessage = "Sorry, at least one of those toppings is not available.")]
		// Bad if you have Extra Ham but not Ham Bad if: !Ham && ExtraHam Therefore, good if: Ham || !ExtraHam
		public IEnumerable<int> Toppings { get; set; }
		public string ToppingsUrl { get { return url.HomeToppingsCheck(); } }
		public IEnumerable<SelectListItem> ToppingsMultiSelectList {
			get {
				return new[] {
					new SelectListItem {Value = "1", Text = "Ham"},
					new SelectListItem {Value = "2", Text = "Pineapple"},
					new SelectListItem {Value = "3", Text = "Pepperoni"},
					new SelectListItem {Value = "4", Text = "Mushrooms"},
					new SelectListItem {Value = "5", Text = "Extra Ham"},
					new SelectListItem {Value = "6", Text = "Anchovies"},
				};
			}
		}

		[Display(Name = "Delivery Date")]
		[ValidIf("['gte','Today']", ErrorMessage = "Must be today or later.")]
		[ValidIf2("['remote','DeliveryDateUrl']", ErrorMessage = "Sorry, that delivery date is not available.")]
		public DateTime? DeliveryDate { get; set; }
		public string DeliveryDateUrl { get { return url.HomeDeliveryDateCheck(); } }

		public DateTime Today { get { return DateTime.Today; } }
	}
}