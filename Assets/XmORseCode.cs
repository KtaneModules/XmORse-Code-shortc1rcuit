/*MESSAGE TO ANY FUTURE CODERS:
 PLEASE COMMENT YOUR WORK
 I can't stress how important this is especially with bomb types such as boss modules.
 If you don't it makes it realy hard for somone like me to find out how a module is working so I can learn how to make my own.
 Please comment your work.
 Short_c1rcuit*/

using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using rnd = UnityEngine.Random;

public class XmORseCode : MonoBehaviour
{
	public KMAudio audio;
	public KMBombInfo bomb;

	//The alphabet and their corresponding morse code characters
	readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	readonly string[] morseTable = { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--..", "-----", ".----", "..---", "...--", "....-", ".....", "-....", "--...", "---..", "----." };
	
	//Possble words and their corresponding orders
	readonly string[] words = { "ADMIT", "AWARD", "BANJO", "BRAVO", "CHILL", "CYCLE", "DECOR", "DISCO", "EERIE", "ERUPT", "FEWER", "FUZZY", "GERMS", "GUSTO", "HAULT", "HEXED", "ICHOR", "INFER", "JEWEL", "KTANE", "LADLE", "LYRIC", "MANGO", "MUTED", "NERDS", "NIXIE", "OOZED", "OXIDE", "PARTY", "PURSE", "QUEST", "RETRO", "ROUGH", "SCOWL", "SIXTH", "THANK", "TWINE", "UNBOX", "USHER", "VIBES", "VOICE", "WHIZZ", "WRUNG", "XENON", "YOLKS", "ZILCH" };
	readonly int[,] orders = { { 4, 2, 3, 5, 1 }, { 2, 5, 3, 4, 1 }, { 4, 2, 1, 5, 3 }, { 3, 2, 4, 5, 1 }, { 3, 2, 5, 4, 1 }, { 4, 1, 5, 2, 3 }, { 3, 1, 5, 4, 2 }, { 1, 2, 5, 3, 4 }, { 5, 3, 2, 1, 4 }, { 4, 3, 1, 2, 5 }, { 2, 4, 5, 3, 1 }, { 2, 1, 4, 5, 3 }, { 4, 1, 3, 2, 5 }, { 2, 3, 4, 5, 1 }, { 4, 5, 3, 2, 1 }, { 1, 2, 5, 3, 4 }, { 1, 3, 5, 4, 2 }, { 3, 1, 4, 5, 2 }, { 3, 1, 5, 2, 4 }, { 5, 4, 1, 2, 3 }, { 1, 5, 3, 2, 4 }, { 3, 5, 2, 1, 4 }, { 5, 1, 4, 3, 2 }, { 1, 5, 4, 3, 2 }, { 5, 4, 3, 1, 2 }, { 2, 4, 3, 5, 1 }, { 1, 4, 2, 5, 3 }, { 1, 3, 4, 2, 5 }, { 2, 5, 3, 1, 4 }, { 1, 4, 5, 3, 2 }, { 1, 2, 3, 4, 5 }, { 4, 2, 1, 3, 5 }, { 1, 4, 5, 2, 3 }, { 4, 1, 3, 5, 2 }, { 4, 3, 1, 5, 2 }, { 5, 3, 2, 1, 4 }, { 4, 2, 5, 1, 3 }, { 2, 3, 5, 1, 4 }, { 5, 3, 2, 1, 4 }, { 4, 1, 5, 3, 2 }, { 1, 4, 2, 3, 5 }, { 1, 3, 4, 2, 5 }, { 1, 2, 4, 5, 3 }, { 2, 1, 4, 3, 5 }, { 3, 2, 4, 5, 1 }, { 4, 5, 3, 1, 2 } };
	
	//Nmaes of the sound effects used
	readonly string[] soundEffects = { "Blaster 1", "Blaster 2", "Blaster 3", "Blaster 4", "Blaster 5" };

	//Number of buttons pressed in submission.
	int pressNum = 0;

	//Position in the alphabet of the letters shown on the module
	int[] displayed = new int[5];

	//Position of the answer in the words array
	int answer;
	
	//Each letter's flash sequence (without the number on the end)
	string[] flashes = new string[5];

	//The textmesh for the letters on the module
	public TextMesh[] letters;

	//The selectables
	public KMSelectable[] buttons;

	//Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	//Twitch help message
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Input the order with “!{0} <order>”. For example: “!{0} 54123”";
#pragma warning restore 414

	public IEnumerator ProcessTwitchCommand(string command)
	{
		//Sets all text to lower case and removes any white space at the end
		command = command.ToLowerInvariant().Trim();

		//If the command is made of 5 digits
		if (Regex.IsMatch(command, @"^\d{5}$"))
		{
			//Go through each digit and press the corresponding button
			foreach (char character in command)
			{
				buttons[int.Parse(character.ToString())-1].OnInteract();
				yield return new WaitForSeconds(0.7f);
			}
		}
		else
		{
			yield return "sendtochaterror The command you inputted is invalid.";
		}
	}

	void Awake()
	{
		//More logging stuff
		moduleId = moduleIdCounter++;

		//Takes the letters and gives them their methods
		foreach (KMSelectable button in buttons)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { ButtonPress(pressedButton); return false; };
		}
	}

	// Use this for initialization
	void Start()
	{
		//Picks one of the words at random
		answer = rnd.Range(0, 46);

		//Generates 5 numbers between 1 and 4 that add to the answer's total length in morse
		int[] randomLengths = RandomSum(1, 4, words[answer].Select(x => morseTable[alphabet.IndexOf(x)].Length).Sum(), 5);

		//This loop gives a random letter whose length in morse is equal to the corresponding value in randomLengths
		for (int i = 0; i < 5; i++)
		{
			string[] xLengthMorse = morseTable.Where(x => x.Length == randomLengths[i]).ToArray();
			string randmorse = xLengthMorse[rnd.Range(0, xLengthMorse.Length)];
			displayed[i] = Array.IndexOf(morseTable, randmorse);
		}

		//Performs the mechanic of XmORse to get the concatenated sequence of flashes.
		string concatMorse = XmORse(string.Join("", displayed.Select(x => morseTable[x]).ToArray()), string.Join("", words[answer].Select(x => morseTable[alphabet.IndexOf(x)]).ToArray()));

		//Generates 5 numbers between 1 and 4 that add to the answer's total length in morse
		randomLengths = RandomSum(1, 4, words[answer].Select(x => morseTable[alphabet.IndexOf(x)].Length).Sum(), 5);

		Debug.LogFormat("[XmORse Code #{0}] Displayed letters are {1}.", moduleId, string.Join(", ", displayed.Select(x => alphabet.Substring(x, 1)).ToArray()));

		//Divides the concatenated morse into a set of flashes and displays the letters on the module
		int total = 0;
		for (int i = 0; i < 5; i++)
		{
			flashes[i] = concatMorse.Substring(total, randomLengths[i]);
			total += randomLengths[i];
			letters[i].text = alphabet.Substring(displayed[i], 1);
			Debug.LogFormat("[XmORse Code #{0}] Letter {1}'s morse is {2}, {3}.", moduleId, i + 1, flashes[i], morseTable[morseTable[alphabet.IndexOf(words[answer][i])].Length + 26]);
		}

		Debug.LogFormat("[XmORse Code #{0}] Performing XmORse on {1} and {2} results in {3}.", moduleId, concatMorse, string.Join("", displayed.Select(x => morseTable[x]).ToArray()), string.Join("", words[answer].Select(x => morseTable[alphabet.IndexOf(x)]).ToArray()));

		Debug.LogFormat("[XmORse Code #{0}] {1} is {2} in morse so the button order is {3}.", moduleId, string.Join(" ", words[answer].Select(x => morseTable[alphabet.IndexOf(x)]).ToArray()), words[answer], string.Concat(orders[answer, 0], orders[answer, 1], orders[answer, 2], orders[answer, 3], orders[answer, 4]));

		StartFlashes();
	}

	//This function generates an int array of a specified length, with numbers in a specified range that add up to a specified total
	int[] RandomSum(int min, int max, int total, int length)
	{
		int[] sum = new int[length];
		int localmin, localmax, remaining;

		remaining = total;

		//As a partial array of numbers may make it impossible to complete, another minimum and maximum must be calculated to make sure this doesn't happen
		for (int i = 1; i <= length; i++)
		{
			//The Max and Min functions are there to make sure numbers don't go out of range
			localmin = Math.Max(min, remaining - (max * (length - i)));
			localmax = Math.Min(max, remaining - (min * (length - i)));

			sum[i - 1] = rnd.Range(localmin, localmax + 1);

			remaining -= sum[i - 1];
		}

		//Fisher-Yates shuffle (credit to jasonmarziani who wrote the code: https://gist.github.com/jasonmarziani/7b4769673d0b593457609b392536e9f9)
		// Loops through array
		for (int i = sum.Length - 1; i > 0; i--)
		{
			// Randomize a number between 0 and i (so that the range decreases each time)
			int swap = rnd.Range(0, i);

			// Save the value of the current i, otherwise it'll overright when we swap the values
			int temp = sum[i];

			// Swap the new and old values
			sum[i] = sum[swap];
			sum[swap] = temp;
		}

		return sum;
	}

	string XmORse(string in1, string in2)
	{
		string output = "";

		for (int i = 0; i < in1.Length; i++)
		{
			output += in1[i] == '-' ^ in2[i] == '-' ? "-" : ".";
		}

		return output;
	}

	//Starts a co-routine for each letter
	void StartFlashes()
	{
		for (int x = 0; x < 5; x++)
			StartCoroutine(FlashLight(x));
	}

	IEnumerator FlashLight(int n)
	{
		//Gets the full sequence of flashes
		string character = flashes[n];
		string number = morseTable[morseTable[alphabet.IndexOf(words[answer][n])].Length + 26];

		//Repeats the flashes until the module is solved
		while (!moduleSolved)
		{
			for (int i = 0; i < character.Length; i++)
			{
				letters[n].gameObject.SetActive(true);
				
				if (character[i] == '-')
					yield return new WaitForSeconds(0.6f);
				else
					yield return new WaitForSeconds(0.2f);

				letters[n].gameObject.SetActive(false);
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(0.5f);

			for (int i = 0; i < number.Length; i++)
			{
				letters[n].gameObject.SetActive(true);

				if (number[i] == '-')
					yield return new WaitForSeconds(0.6f);
				else
					yield return new WaitForSeconds(0.2f);

				letters[n].gameObject.SetActive(false);
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	void ButtonPress(KMSelectable button)
	{
		if (moduleSolved) return;

		//Makes the bomb move when you press it
		button.AddInteractionPunch();

		//As the value of each number on the keypad is equivalent to their position in the array, I can get the button's position and use that to work out it's value.
		int position = Array.IndexOf(buttons, button);

		//If the correct button has been pressed
		if (position + 1 == orders[answer, pressNum])
		{
			//Makes the letter vanish
			button.gameObject.SetActive(false);

			//Makes a sound when you press the button.
			audio.PlaySoundAtTransform(soundEffects[pressNum], transform);

			pressNum++;

			Debug.LogFormat("[XmORse Code #{0}] You pressed button {1}. Correct.", moduleId, position + 1);
		}
		//If the incorrect button has been pressed
		else
		{
			//Makes all letters visable again
			foreach (KMSelectable letter in buttons)
			{
				letter.gameObject.SetActive(true);
			}

			//Strikes the bomb
			GetComponent<KMBombModule>().HandleStrike();
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
			pressNum = 0;

			Debug.LogFormat("[XmORse Code #{0}] You pressed button {1}. Incorrect. Resetting input.", moduleId, position + 1);
		}

		//After all buttons have been pressed
		if (pressNum == 5)
		{
			//Solves the module
			Debug.LogFormat("[XmORse Code #{0}] Module solved!", moduleId);
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
		}
	}
}