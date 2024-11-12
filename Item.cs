using System;

[Serializable]
public class Item
{
	public enum CharacterPart
	{
		HAIR = 0,
		UPPER_CLOTHES = 1,
		LOWER_CLOTHES = 2,
		SHOES = 3,
		SET = 4,
		WINGS = 5,
		ACCESSORIES = 6,
		CONSUMABLE = 7,
		CROWN = 8,
		TAIL = 9,
		RIDER = 10,
		EFFECT = 11,
		PET = 12,
		CHARACTER = 13,
		AVATAR = 14,
		CHAT_MESSAGE = 15,
		VIP_WINGS = 16,
		RANGED = 17,
		MELEE = 18,
		SHIELD = 19,
		FOLLOWER = 20
	}

	public enum CurrencyType
	{
		GOLD = 0,
		RUBY = 1
	}

	public enum ItemValueType
	{
		EXP = 0,
		GOLD = 1,
		RUBY = 2,
		COUPLE_EXP = 3,
		CLUB_EXP = 4,
		NONE = 5
	}

	public int itemId;

	public string itemName;

	public string itemType;

	public bool isMale;

	public int price;

	public string currencyType;

	public string valueType;

	public string value;

	public int quantity;

	public string expiryTime;

	public int percentage;

	public int levelRequired;

	public bool inUsed;

	public bool isItemType(CharacterPart part)
	{
		return itemType.Equals(part.ToString());
	}

	public bool isWeapon()
	{
		if (!itemType.Equals(CharacterPart.RANGED.ToString()) && !itemType.Equals(CharacterPart.MELEE.ToString()) && !itemType.Equals(CharacterPart.SHIELD.ToString()))
		{
			return itemType.Equals(CharacterPart.FOLLOWER.ToString());
		}
		return true;
	}

	public static Item groomClothes()
	{
		return new Item
		{
			itemType = CharacterPart.SET.ToString(),
			itemName = "Texudo",
			itemId = 574492
		};
	}

	public static Item brideClothes()
	{
		return new Item
		{
			itemType = CharacterPart.SET.ToString(),
			itemName = "Wedding_Dress_2",
			itemId = 864862
		};
	}

	public static Item defaultClothes(CharacterPart part, bool isMale)
	{
		Item item = new Item();
		item.itemType = part.ToString();
		if (isMale)
		{
			switch (part)
			{
			case CharacterPart.LOWER_CLOTHES:
				item.itemName = "Boy_FlowerTattoos_Trunks";
				item.itemId = 764928;
				item.isMale = true;
				break;
			case CharacterPart.UPPER_CLOTHES:
				item.itemName = "Boy_FlowerTattoos_Body";
				item.itemId = 667476;
				item.isMale = true;
				break;
			}
		}
		else
		{
			switch (part)
			{
			case CharacterPart.LOWER_CLOTHES:
				item.itemName = "Girl_WhiteBikini_Lower";
				item.itemId = 227279;
				item.isMale = false;
				break;
			case CharacterPart.UPPER_CLOTHES:
				item.itemName = "Girl_WhiteBikini_Top";
				item.itemId = 549806;
				item.isMale = false;
				break;
			case CharacterPart.SHOES:
				item.itemName = "Girl_WhiteBikini_Shoes";
				item.itemId = 411108;
				item.isMale = false;
				break;
			case CharacterPart.HAIR:
				item.itemName = "Girl_Hair_001";
				item.itemId = 106455;
				item.isMale = false;
				break;
			}
		}
		return item;
	}
}
