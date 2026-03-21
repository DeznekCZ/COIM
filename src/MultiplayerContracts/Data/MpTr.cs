using Mafi.Localization;

namespace MultiplayerContracts.Data;

public class MpTr {
	public static readonly LocStr MarketNotConnected = Loc.Str($"{nameof(MpTr)}_{nameof(MarketNotConnected)}",
		"No market connected", "the ip address is not correct");

	public static readonly LocStr SearchingOffers = Loc.Str($"{nameof(MpTr)}_{nameof(SearchingOffers)}",
		"Searching for product offers", "progress of searching of offers");

	public static readonly LocStr TradingProgress = Loc.Str($"{nameof(MpTr)}_{nameof(TradingProgress)}",
		"Trade in progress", "progress of trading of offer");

	public static readonly LocStr ClaimingProgress = Loc.Str($"{nameof(MpTr)}_{nameof(TradingProgress)}",
		"Claim in progress", "progress of claiming of offer");

	public static readonly LocStr UpdateOffers = Loc.Str($"{nameof(MpTr)}_{nameof(UpdateOffers)}",
		"Update list of items", "start searching of offers");

	public static readonly LocStr FailedSearchingOffers = Loc.Str($"{nameof(MpTr)}_{nameof(FailedSearchingOffers)}",
		"Market does not responding", "start searching of offers");

	public static readonly LocStr Revocable = Loc.Str($"{nameof(MpTr)}_{nameof(Revocable)}",
		"Revocable", "list of offers that was created by player and can be removed from offers back");

	public static readonly LocStr Claimable = Loc.Str($"{nameof(MpTr)}_{nameof(Claimable)}",
		"Claimable", "list of offers that was created by player and can was accepted by other player");

	public static readonly LocStr Market = Loc.Str($"{nameof(MpTr)}_{nameof(Market)}",
		"Market", "name of field to enter server address");

	public static readonly LocStr Storage = Loc.Str($"{nameof(MpTr)}_{nameof(Storage)}",
		"Storage", "a tab with list of stored products");

	public static readonly LocStr Storage_tooltip = Loc.Str($"{nameof(MpTr)}_{nameof(Storage_tooltip)}",
		"Shows all products stored in inventory of your dock", "a tab with list of stored products");

	public static readonly LocStr MyOffers = Loc.Str($"{nameof(MpTr)}_{nameof(MyOffers)}",
		"My offers", "a tab with offers made by owner of building");

	public static readonly LocStr MyOffers_tooltip = Loc.Str($"{nameof(MpTr)}_{nameof(MyOffers_tooltip)}",
		"Shows all offers that you sent to selected market", "a tab with offers made by owner of building");

	public static readonly LocStr OnlineOffers = Loc.Str($"{nameof(MpTr)}_{nameof(OnlineOffers)}",
		"Market offers", "a tab with offers made by other players");

	public static readonly LocStr OnlineOffers_tooltip = Loc.Str($"{nameof(MpTr)}_{nameof(OnlineOffers_tooltip)}",
		"Shows all offers made by other players", "a tab with offers made by other players");

	public static readonly LocStr1Plural AvailableOffersOfType = Loc.Str1Plural($"{nameof(MpTr)}_{nameof(AvailableOffersOfType)}",
		"One time offer.", "This offer is available for {0} uses.", "a tab with offers made by other players");

	public static readonly LocStr QuantityAssertion_OfferAboveZero = Loc.Str($"{nameof(MpTr)}_{nameof(QuantityAssertion_OfferAboveZero)}",
		"Offer must be greater than zero", "number greater than zero");

	public static readonly LocStr QuantityAssertion_DemandAboveZero = Loc.Str($"{nameof(MpTr)}_{nameof(QuantityAssertion_DemandAboveZero)}",
		"Demand must be greater than zero", "number greater than zero");

	public static readonly LocStr1 QuantityAssertion_OfferAboveCapacity = Loc.Str1($"{nameof(MpTr)}_{nameof(QuantityAssertion_OfferAboveCapacity)}",
		"Cannot make offer bigger than capacity: {0}", "number greater than zero");

	public static readonly LocStr1 QuantityAssertion_DemandAboveCapacity = Loc.Str1($"{nameof(MpTr)}_{nameof(QuantityAssertion_DemandAboveCapacity)}",
		"Cannot make demand bigger than capacity: {0}", "number greater than zero");
}
