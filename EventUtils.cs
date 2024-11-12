public class EventUtils
{
	public static void triggerUserUpdatedEvent(bool updateClothes)
	{
		CommonUtils.GetMainController().triggerUserUpdatedEvent(updateClothes);
	}
}
