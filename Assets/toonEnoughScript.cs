﻿using System;
using System.Collections.Generic;
using KModkit;
using UnityEngine;

public class toonEnoughScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMSelectable yesButton;
    public KMSelectable noButton;
    public KMSelectable toonPic;
    public Texture[] toons;
    public Texture[] cogs;
    public Renderer toonScreen;
    public Renderer cogScreen;
    public TextMesh chat;

    private static readonly string[] phrases =
    {
        "What goes 'Ha Ha Ha Thud'?\nSomeone laughing his head off.",
        "What goes TICK-TOCK-WOOF?\nA watchdog!",
        "Why do male deer need braces?\nBecause they have 'buck teeth'!",
        "What has 1 horn and gives milk?\nA milk truck!",
        "When is the vet busiest?\nWhen it's raining cats and dogs.",
        "What's the best parting gift?\nA comb.",
        "Why did the man hit the clock?\nBecause the clock struck first.",
        "What goes dot-dash-squeak?\nMouse code.",
        "How do trains hear?\nThrough their engineers.",
        "What has six eyes but cannot see?\nThree blind mice.",
        "Keep your eyes on the doughnut,\nnot the hole.",
        "What works only when it's fired?\nA ROCKET.",
        "What goes Oh, Oh, Oh?\nSanta walking backwards!",
        "What do mermaids have on toast?\nMermarlade.",
        "My friend thinks he's a rubber\nband. I told him to snap out of it.",
        "How does a sick sheep feel?\nBaah-aahd.",
        "Why did the dog chase his tail?\nTo make ends meet.",
        "What goes zzub-zzub?\nA bee flying backward.\n",
        "How do you clean a tuba?\nWith a tuba toothpaste.",
        "What do frogs like to sit on?\nToadstools.",
        "What's a polygon?\nA dead parrot.",
        "What's a funny egg called?\nA practical yolker."
    };

    private static readonly int[,] laffChart = new int[,]
    {
        {76, 124, 79, 107, 113, 127, 26, 75, 61, 77, 43},
        {92, 21, 125, 28, 71, 119, 82, 85, 101, 16, 69},
        {23, 44, 100, 87, 136, 105, 49, 68, 81, 89, 118},
        {56, 41, 39, 51, 37, 114, 65, 58, 18, 47, 93},
        {133, 31, 104, 73, 67, 134, 98, 34, 122, 62, 40},
        {115, 106, 128, 130, 112, 45, 63, 129, 123, 22, 57},
        {27, 48, 99, 91, 90, 55, 60, 80, 19, 116, 36},
        {15, 95, 86, 88, 72, 24, 102, 59, 53, 94, 135},
    };
    private static readonly int[,] gagChart = new int[,]
    {
        {1, 1, 2, 3, 5, 8, 13},
        {2, 3, 4, 5, 10, 15, 20},
        {1, 3, 7, 14, 20, 26, 36},
        {2, 4, 8, 16, 24, 32, 40},
        {2, 4, 8, 12, 18, 24, 32},
        {1, 3, 5, 9, 15, 21, 29},
        {2, 3, 4, 5, 10, 15, 20}
    };

    private static readonly Dictionary<string, float> aspectRatios = new Dictionary<string, float>
    {
        { "1_Story_Building", 1.56640625f },
        { "2_Story_Building", 1.72189349112426f },
        { "3_Story_Building", 1.83196721311475f },
        { "4_Story_Building", 1.60498220640569f },
        { "5_Story_Building", 1.74545454545455f },
        { "The_Back_Nine", 2.14420062695925f },
        { "The_Cashbot_Bullion_Mint", 1.3648393194707f },
        { "The_Cashbot_Coin_Mint", 1.12008733624454f },
        { "The_Cashbot_Dollar_Mint", 1.28571428571429f },
        { "The_CEO", 1.07267441860465f },
        { "The_CFO", 1.02409638554217f },
        { "The_CJ", 1.27727272727273f },
        { "The_Front_Three", 2.27358490566038f },
        { "The_Lawbot_A_Office", 0.951977401129944f },
        { "The_Lawbot_B_Office", 0.937142857142857f },
        { "The_Lawbot_C_Office", 0.946608946608947f },
        { "The_Lawbot_D_Office", 1f },
        { "The_Middle_Six", 2.26100628930818f },
        { "The_Sellbot_Factory", 3.04347826086957f },
        { "The_VP", 1.29365079365079f }
    };

    private Texture toon;
    private Texture cog;
    private string[] toonDetails;

    private string phrase;
    private int phraseType;
    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private int laff;
    private bool answer;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        yesButton.OnInteract += delegate () { YesButton(); return false; };
        noButton.OnInteract += delegate () { NoButton(); return false; };
        toonPic.OnInteract += delegate () { toonButton(); return false; };
    }
    // Use this for initialization
    void Start()
    {
        phrase = phrases[UnityEngine.Random.Range(0, phrases.Length)];
        phraseType = 0;
        chat.text = phrase;
        toon = toons[UnityEngine.Random.Range(0, toons.Length)];
        toonScreen.material.mainTexture = toon;
        toonDetails = toon.name.Split('_');
        Debug.LogFormat("[Toon Enough #{0}] Species: {1}, Color: {2}, Gender: {3}", moduleId, toonDetails[1], toonDetails[0], toonDetails[2]);
        getLaff();
        getCog();
        Debug.LogFormat("[Toon Enough #{0}] Cog Challenge: {1}", moduleId, cog.name.Replace("_", " "));
        cogScreen.GetComponent<Renderer>().material.mainTexture = cog;
        if (0.059f * aspectRatios[cog.name] > .113f)
            cogScreen.transform.localScale = new Vector3(.113f, 0, .113f / aspectRatios[cog.name]);
        else
            cogScreen.transform.localScale = new Vector3(0.059f * aspectRatios[cog.name], 0, 0.059f);

        Debug.LogFormat("[Toon Enough #{0}] Laff: {1}", moduleId, laff);

        string[] gags = getGags();
        string[] gagsCarry;
        if (laff >= 61)
            gagsCarry = new string[6];
        else if (laff >= 52)
            gagsCarry = new string[5];
        else if (laff >= 34)
            gagsCarry = new string[4];
        else if (laff >= 25)
            gagsCarry = new string[3];
        else
            gagsCarry = new string[2];
        for (int aa = 0; aa < gagsCarry.Length; aa++)
        {
            gagsCarry[aa] = gags[aa];
        }

        int[] gagLevels = getGagLevels(gagsCarry);

        int toonScore = getToonScore(gagsCarry, gagLevels);
        int cogScore = getCogScore();
        Debug.LogFormat("[Toon Enough #{0}] Final Toon Score: {1}", moduleId, toonScore);
        Debug.LogFormat("[Toon Enough #{0}] Cog Challenge Score: {1}", moduleId, cogScore);
        answer = toonScore >= cogScore;
        Debug.LogFormat("[Toon Enough #{0}] Answer should be {1}", moduleId, answer ? "YES" : "NO");
    }

    void getLaff()
    {
        int col = -1;
        int row = -1;
        switch (toonDetails[0])
        {
            case "Red":
                row = 0;
                break;
            case "Orange":
                row = 1;
                break;
            case "Yellow":
                row = 2;
                break;
            case "Green":
                row = 3;
                break;
            case "Blue":
                row = 4;
                break;
            case "Purple":
                row = 5;
                break;
            case "Pink":
                row = 6;
                break;
            case "Brown":
                row = 7;
                break;
        }
        switch (toonDetails[1])
        {
            case "Cat":
                col = 0;
                break;
            case "Dog":
                col = 1;
                break;
            case "Duck":
                col = 2;
                break;
            case "Rabbit":
                col = 3;
                break;
            case "Horse":
                col = 4;
                break;
            case "Pig":
                col = 5;
                break;
            case "Monkey":
                col = 6;
                break;
            case "Mouse":
                col = 7;
                break;
            case "Bear":
                col = 8;
                break;
            case "Deer":
                col = 9;
                break;
            case "Crocodile":
                col = 10;
                break;
        }
        laff = laffChart[row, col];
    }
    void getCog()
    {
        if (laff >= 100)
            cog = cogs[UnityEngine.Random.Range(0, cogs.Length)];
        else if (laff >= 96)
            cog = cogs[UnityEngine.Random.Range(0, 17)];
        else if (laff >= 95)
            cog = cogs[UnityEngine.Random.Range(0, 16)];
        else if (laff >= 90)
            cog = cogs[UnityEngine.Random.Range(0, 15)];
        else if (laff >= 86)
            cog = cogs[UnityEngine.Random.Range(0, 14)];
        else if (laff >= 81)
            cog = cogs[UnityEngine.Random.Range(0, 13)];
        else if (laff >= 76)
            cog = cogs[UnityEngine.Random.Range(0, 12)];
        else if (laff >= 71)
            cog = cogs[UnityEngine.Random.Range(0, 11)];
        else if (laff >= 66)
            cog = cogs[UnityEngine.Random.Range(0, 9)];
        else if (laff >= 61)
            cog = cogs[UnityEngine.Random.Range(0, 8)];
        else
            cog = cogs[UnityEngine.Random.Range(0, 7)];
    }
    string[] getGags()
    {
        string[] gags = new string[6];
        gags[0] = "Throw";
        gags[1] = "Squirt";
        if (toonDetails[1].EqualsIgnoreCase("dog") || toonDetails[1].EqualsIgnoreCase("rabbit") || toonDetails[1].EqualsIgnoreCase("horse") || toonDetails[1].EqualsIgnoreCase("monkey") || toonDetails[1].EqualsIgnoreCase("mouse") || toonDetails[1].EqualsIgnoreCase("bear"))
        {
            gags[2] = "Toon-up";
            if (toonDetails[0].EqualsIgnoreCase("brown") || toonDetails[0].EqualsIgnoreCase("purple") || toonDetails[0].EqualsIgnoreCase("yellow") || toonDetails[0].EqualsIgnoreCase("green"))
            {
                gags[3] = "Drop";
                if (toonDetails[2].EqualsIgnoreCase("male"))
                {
                    gags[4] = "Sound";
                    if (laff > 97)
                    {
                        gags[5] = "Trap";
                    }
                    else
                    {
                        gags[5] = "Lure";
                    }
                }
                else
                {
                    gags[4] = "Trap";
                    if (laff > 97)
                    {
                        gags[5] = "Lure";
                    }
                    else
                    {
                        gags[5] = "Sound";
                    }
                }
            }
            else
            {
                gags[3] = "Lure";
                if (toonDetails[2].EqualsIgnoreCase("male"))
                {
                    gags[4] = "Sound";
                    if (laff > 97)
                    {
                        gags[5] = "Drop";
                    }
                    else
                    {
                        gags[5] = "Trap";
                    }
                }
                else
                {
                    gags[4] = "Trap";
                    if (laff > 97)
                    {
                        gags[5] = "Drop";
                    }
                    else
                    {
                        gags[5] = "Sound";
                    }
                }
            }
        }
        else
        {
            gags[2] = "Sound";
            if (toonDetails[0].EqualsIgnoreCase("brown") || toonDetails[0].EqualsIgnoreCase("purple") || toonDetails[0].EqualsIgnoreCase("yellow") || toonDetails[0].EqualsIgnoreCase("green"))
            {
                gags[3] = "Drop";
                if (toonDetails[2].EqualsIgnoreCase("male"))
                {
                    gags[4] = "Toon-up";
                    if (laff > 97)
                    {
                        gags[5] = "Trap";
                    }
                    else
                    {
                        gags[5] = "Lure";
                    }
                }
                else
                {
                    gags[4] = "Trap";
                    if (laff > 97)
                    {
                        gags[5] = "Lure";
                    }
                    else
                    {
                        gags[5] = "Toon-up";
                    }
                }
            }
            else
            {
                gags[3] = "Lure";
                if (toonDetails[2].EqualsIgnoreCase("male"))
                {
                    gags[4] = "Toon-up";
                    if (laff > 97)
                    {
                        gags[5] = "Drop";
                    }
                    else
                    {
                        gags[5] = "Trap";
                    }
                }
                else
                {
                    gags[4] = "Trap";
                    if (laff > 97)
                    {
                        gags[5] = "Drop";
                    }
                    else
                    {
                        gags[5] = "Toon-up";
                    }
                }
            }
        }
        return gags;
    }
    int[] getGagLevels(string[] gc)
    {
        String sn = Bomb.GetSerialNumber();
        int[] gl = new int[gc.Length];
        int num = laff;
        while (num > 9)
        {
            String c = num + "";
            num = 0;
            foreach (char d in c)
            {
                num += (d - '0');
            }
        }
        num += (sn[sn.Length - 1] - '0');
        gl[0] = (num % 7) + 1;
        Debug.LogFormat("[Toon Enough #{0}] Throw Level: (({1} + DR({2})) % 7) + 1 = {3}", moduleId, sn[sn.Length - 1], laff, gl[0]);
        if (gl[0] < 5 && laff >= 52)
        {
            Debug.LogFormat("[Toon Enough #{0}] Throw level is below 5 and laff is 52 or greater, setting Throw level to 5.", moduleId);
            gl[0] = 5;
        }

        sn = sn.Substring(0, 5);
        int cur = 2;
        for (int aa = 1; aa >= 0; aa--)
        {
            switch (sn[aa])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    cur = aa;
                    break;
            }
        }
        gl[1] = (((sn[cur] - '0') + (laff % 10)) % 7) + 1;
        Debug.LogFormat("[Toon Enough #{0}] Squirt Level: (({1} + ({2} % 10)) % 7) + 1 = {3}", moduleId, sn[cur], laff, gl[1]);
        sn = sn.Substring(0, cur) + "" + sn.Substring(cur + 1);
        for (int bb = 2; bb < gl.Length; bb++)
        {
            switch (sn[bb - 2])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    num = sn[bb - 2] - '0';
                    break;
                case 'A':
                    num = 10;
                    break;
                case 'B':
                    num = 11;
                    break;
                case 'C':
                    num = 12;
                    break;
                case 'D':
                    num = 13;
                    break;
                case 'E':
                    num = 14;
                    break;
                case 'F':
                    num = 15;
                    break;
                case 'G':
                    num = 16;
                    break;
                case 'H':
                    num = 17;
                    break;
                case 'I':
                    num = 18;
                    break;
                case 'J':
                    num = 19;
                    break;
                case 'K':
                    num = 20;
                    break;
                case 'L':
                    num = 21;
                    break;
                case 'M':
                    num = 22;
                    break;
                case 'N':
                    num = 23;
                    break;
                case 'O':
                    num = 24;
                    break;
                case 'P':
                    num = 25;
                    break;
                case 'Q':
                    num = 26;
                    break;
                case 'R':
                    num = 27;
                    break;
                case 'S':
                    num = 28;
                    break;
                case 'T':
                    num = 29;
                    break;
                case 'U':
                    num = 30;
                    break;
                case 'V':
                    num = 31;
                    break;
                case 'W':
                    num = 32;
                    break;
                case 'X':
                    num = 33;
                    break;
                case 'Y':
                    num = 34;
                    break;
                case 'Z':
                    num = 35;
                    break;
            }
            gl[bb] = (num % 7) + 1;
            Debug.LogFormat("[Toon Enough #{0}] {1} Level: ({2} % 7) + 1 = {3}", moduleId, gc[bb], num, gl[bb]);
        }
        return gl;
    }
    int getToonScore(string[] gc, int[] gl)
    {
        int ts = 0;
        String scoreOutput = "Final Toon Score: ";
        for (int aa = 0; aa < gc.Length; aa++)
        {
            int row = -1;
            switch (gc[aa])
            {
                case "Toon-up":
                    row = 0;
                    break;
                case "Trap":
                    row = 1;
                    break;
                case "Lure":
                    row = 2;
                    break;
                case "Sound":
                    row = 3;
                    break;
                case "Throw":
                    row = 4;
                    break;
                case "Squirt":
                    row = 5;
                    break;
                case "Drop":
                    row = 6;
                    break;
            }
            ts += gagChart[row, gl[aa] - 1];
            scoreOutput = scoreOutput + "" + gagChart[row, gl[aa] - 1] + " + ";
        }
        ts += (laff / 10);
        scoreOutput = scoreOutput + "" + (laff / 10) + " + ";
        bool uber = false;
        if (gc.Length == 2)
        {
            if ((gl[0] >= 6 && gl[1] >= 4) || (gl[1] >= 6 && gl[0] >= 4))
                uber = true;
        }
        else if (gc.Length == 3)
        {
            int num6 = 0;
            for (int dd = 0; dd < 3; dd++)
            {
                if (gl[dd] >= 6)
                    num6++;
            }
            if (num6 > 1)
                uber = true;
        }
        else if (gc.Length == 4)
        {
            int num6 = 0;
            int num5 = 0;
            for (int dd = 0; dd < 4; dd++)
            {
                if (gl[dd] >= 6)
                    num6++;
                else if (gl[dd] == 5)
                    num5++;
            }
            if (num6 > 1)
                uber = true;
            else if (num6 == 1 && num5 > 1)
                uber = true;
        }
        if (uber)
        {
            Debug.LogFormat("[Toon Enough #{0}] Is an uber!", moduleId);
            ts += 25;
            scoreOutput = scoreOutput + "25 + ";
        }

        if (cog.name.ContainsIgnoreCase("Building") && Bomb.GetBatteryCount() >= 3)
        {
            Debug.LogFormat("[Toon Enough #{0}] Toon is doing a building invasion!", moduleId);
            scoreOutput = scoreOutput + "20 + ";
            ts += 20;
        }
        scoreOutput = scoreOutput.Substring(0, scoreOutput.Length - 2);
        Debug.LogFormat("[Toon Enough #{0}] {1} = {2}", moduleId, scoreOutput, ts);
        return ts;
    }
    int getCogScore()
    {
        switch (cog.name)
        {
            case "1_Story_Building":
                return 58;
            case "2_Story_Building":
                return 60;
            case "3_Story_Building":
                return 63;
            case "4_Story_Building":
                return 66;
            case "5_Story_Building":
                return 69;
            case "The_Sellbot_Factory":
                return 65;
            case "The_Cashbot_Coin_Mint":
                return 78;
            case "The_Cashbot_Dollar_Mint":
                return 80;
            case "The_Cashbot_Bullion_Mint":
                return 82;
            case "The_Lawbot_A_Office":
                return 79;
            case "The_Lawbot_B_Office":
                return 81;
            case "The_Lawbot_C_Office":
                return 83;
            case "The_Lawbot_D_Office":
                return 85;
            case "The_Front_Three":
                return 85;
            case "The_Middle_Six":
                return 87;
            case "The_Back_Nine":
                return 89;
            case "The_VP":
                return 73;
            case "The_CFO":
                return 85;
            case "The_CJ":
                return 87;
            case "The_CEO":
                return 90;
            default:
                return -1;
        }
    }
    void YesButton()
    {
        if (!moduleSolved)
        {
            yesButton.AddInteractionPunch();
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (answer)
            {
                Audio.PlaySoundAtTransform("solved", transform);
                string[] toonSayings = { "OMG, YAY! I thought so but\nI wasn't sure! Thank you!", "OOOO! THAT'S WHAT\nI'M TALKING ABOUT!!!" };
                chat.text = toonSayings[UnityEngine.Random.Range(0, 2)];
                Module.HandlePass();
                moduleSolved = true;
            }
            else
            {
                Audio.PlaySoundAtTransform("strike", transform);
                Module.HandleStrike();
                Start();
            }
        }

    }
    void NoButton()
    {
        if (!moduleSolved)
        {
            noButton.AddInteractionPunch();
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (!answer)
            {
                Audio.PlaySoundAtTransform("solved", transform);
                string[] toonSayings = { "Aww man! I guess I'll have to\nwork harder.. *sigh*", "Really? You're kidding.. Alright.\nThanks anyways!" };
                chat.text = toonSayings[UnityEngine.Random.Range(0, 2)];
                Module.HandlePass();
                moduleSolved = true;
            }
            else
            {
                Audio.PlaySoundAtTransform("strike", transform);
                Module.HandleStrike();
                Start();
            }
        }
    }

    void toonButton()
    {
        if (!moduleSolved)
        {
            toonPic.AddInteractionPunch();
            Audio.PlaySoundAtTransform(toonDetails[1] + "Voice", transform);
            if (phraseType == 0)
            {

                chat.text = "My color is " + toonDetails[0] + ".";
                phraseType = 1;
            }
            else
            {
                chat.text = phrase;
                phraseType = 0;
            }
        }
        else if (phraseType < 2)
        {
            toonPic.AddInteractionPunch();
            Audio.PlaySoundAtTransform(toonDetails[1] + "Voice", transform);
            chat.text = "Why are you clicking on me?\nI'm already solved!";
            phraseType = 2;
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} yes/no [presses the yes or no button] | !{0} toon [toggles colorblind mode]";
#pragma warning restore 414
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        if (command.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { yesButton };
        }
        else if (command.Equals("no", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { noButton };
        }
        else if (command.Equals("toon", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { toonPic };
        }
        return null;
    }
}
