﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TableOfContents : MonoBehaviour
{

    Transform topMid;
    GameObject journalContainer;
    [SerializeField]
    GameObject journalEntryPage;
    public GameObject jornalEntryButton;
    public Text JournalText;

    [SerializeField]
    GameObject openJournal;
    public Animator flasher;

    Image JournalBackground;

    public List<TableOfContentsButton> goToJournalButtons = new List<TableOfContentsButton>();
    public JournalSaveObjects SaveGame;

    string dataPath;


    // Start is called before the first frame update
    void Start()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "SaveGame.txt");
        LoadJournal();

        if (SceneManager.GetActiveScene().buildIndex > SaveGame.levelsUnlocked)
        {
            SaveGame.levelsUnlocked = SceneManager.GetActiveScene().buildIndex;
            SaveJournal();
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            // Why are we resetting the data on re-opens?
            SaveGame.CollectedObjects.Clear();
            SaveGame.levelsUnlocked = 1;
            SaveJournal();
        }


        JournalBackground = GetComponent<Image>();
        journalContainer = transform.GetChild(0).gameObject;
        topMid = journalContainer.transform.Find("TopMid");
        flasher = transform.parent.Find("OpenJournal").GetComponent<Animator>();
        //journalEntryPage = journalContainer.transform.Find("JournalPage").gameObject;
        CloseJournalEntry();
        CloseJournal();

    }

    public void AddNewJournalEntry(string JournalToLoad)
    {
        SaveGame.CollectedObjects.Add(Resources.Load<JournalEntryObject>("scriptableObjects/Journal_Entries/" + JournalToLoad));
        SaveJournal();
        flasher.SetTrigger("Flash");
        if (!flasher.GetComponent<AudioSource>().isPlaying)
            flasher.GetComponent<AudioSource>().Play();
    }

    public void GoToJournalEntry(string _JournalEntryName)
    {
        foreach (JournalEntryObject jeo in SaveGame.CollectedObjects)
        {
            if (jeo.JournalEntryName == _JournalEntryName)
            {
                journalEntryPage.SetActive(true);
                JournalText.text = jeo.JournalEntryText;
            }
        }
    }


    public void GoBackButton()
    {
        if (journalEntryPage.activeInHierarchy)
            CloseJournalEntry();
        else
            CloseJournal();
    }

    public void CloseJournalEntry()
    {
        journalEntryPage.SetActive(false);
    }

    public void UpdateJournalEntries()
    {
        foreach (JournalEntryObject jeo in SaveGame.CollectedObjects)
        {
            bool exists = false;

            foreach (TableOfContentsButton tocb in goToJournalButtons)
            {
                if (tocb.JournalEntryName == jeo.JournalEntryName)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                GameObject g = Instantiate(jornalEntryButton, journalContainer.transform);
                if (goToJournalButtons.Count > 0)
                    g.transform.position = goToJournalButtons[goToJournalButtons.Count - 1].transform.position + new Vector3(0, -150, 0);
                else
                    g.transform.position = topMid.position + new Vector3(0, -150, 0);
                goToJournalButtons.Add(g.GetComponent<TableOfContentsButton>());
                g.GetComponent<TableOfContentsButton>().UpdateText(this, jeo.JournalEntryName);
                g.transform.SetAsFirstSibling();
            }
        }
    }

    public void OpenJournal()
    {
        if (journalEntryPage.activeInHierarchy)
            CloseJournalEntry();
        else
        {

            JournalBackground.enabled = true;
            journalContainer.SetActive(true);
        }

        UpdateJournalEntries();
    }

    public void CloseJournal()
    {
        JournalBackground.enabled = false;
        journalContainer.SetActive(false);
    }

    public void SaveJournal()
    {

        string jsonString = JsonUtility.ToJson(SaveGame);
        using (StreamWriter streamWriter = File.CreateText(dataPath))
        {
            streamWriter.Write(jsonString);
        }
    }

    public void LoadJournal()
    {
        if (File.Exists(dataPath))
        {
            using (StreamReader streamReader = File.OpenText(dataPath))
            {
                string jsonString = streamReader.ReadToEnd();
                JsonUtility.FromJsonOverwrite(jsonString, SaveGame);
            }
        }
    }

}
