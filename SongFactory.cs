using System.Collections.Generic;
using Lean.Localization;
using UnityEngine;

public class SongFactory : MonoBehaviour
{
	private static List<Song> songs;

	public static List<int> bubbleSongIds = new List<int> { 4, 5, 10, 23, 24, 27, 45, 48, 51, 62 };

	public static List<Song> getSongList()
	{
		if (songs == null)
		{
			string text = "Audio/Songs/";
			songs = new List<Song>();
			string translationText = LeanLocalization.GetTranslationText("Random");
			songs.Add(new Song(1, translationText, "--", translationText, "--", "--", ""));
			songs.Add(new Song(19, "RISE", "LOL Champion 2018", "140", "4", "03:30", text + "140-RISE"));
			songs.Add(new Song(20, "Unravel", "Tokyo Ghoul", "135", "3", "04:01", text + "135-Unravel"));
			songs.Add(new Song(36, "Better Now", "Nighcore", "130", "3", "03:11", text + "130-Better_Now"));
			songs.Add(new Song(69, "Crescent Bay", "Tiktok", "90", "1", "03:15", text + "90-Crescent_Bay"));
			songs.Add(new Song(73, "Lo Say Bye La Bye", "LEMESE x CHANGG", "100", "1", "03:34", text + "100-Lo_Say_Bye_La_Bye"));
			songs.Add(new Song(74, "Khong Yeu Cung Chang Co Don", "Cody", "105", "2", "04:42", text + "105-Khong_Yeu_Cung_Chang_Co_Don"));
			songs.Add(new Song(76, "Cocoon Breaking", "Angela Chang", "105", "2", "03:28", text + "105-破茧"));
			songs.Add(new Song(77, "Bu She", "Lala Hsu", "95", "1", "04:32", text + "95-Bu_She"));
			songs.Add(new Song(79, "Đau Nhâ\u0301t La\u0300 Lă\u0323ng Im Remix", "ERIK", "120", "2", "03:39", text + "120-Dau_Nhat_La_Lang_Yeu"));
			songs.Add(new Song(80, "Anh Đa\u0303 La\u0323c Va\u0300o Remix", "Green ft. Truzg", "120", "2", "02:32", text + "120-Anh_Da_Lac_Vao"));
			songs.Add(new Song(81, "See Ti\u0300nh Remix", "Hoa\u0300ng Thu\u0300y Linh", "122", "3", "02:50", text + "122-See_Ti\u0300nh"));
			songs.Add(new Song(82, "Stay Remix", "Justin Bieber", "132", "3", "02:04", text + "132-Stay"));
			songs.Add(new Song(83, "Đa\u0303 Quên Hay Chưa", "王靖雯不胖", "105", "2", "04:20", text + "105-Da_Quen_Hay_Chua"));
			songs.Add(new Song(84, "Tro\u0300 Chuyê\u0323n", "王靖雯不胖", "100", "2", "03:44", text + "100_Tro_Chuyen"));
		}
		return songs;
	}

	public static Song randomSong()
	{
		return getSongList()[Random.Range(1, getSongList().Count)];
	}
}
