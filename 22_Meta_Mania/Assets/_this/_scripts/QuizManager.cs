using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuizManager : MonoBehaviour
{
    [Serializable]
    public class QuizQuestion
    {
        public int questionIndex;
        public string questionText;
        public List<Option> options;
        public int correctAnswerIndex;

        public QuizQuestion(int qi, string question, List<Option> opts, int correctIndex)
        {
            questionIndex = qi;
            questionText = question;
            options = opts;
            correctAnswerIndex = correctIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is QuizQuestion other)
            {
                return questionIndex == other.questionIndex &&
                       questionText == other.questionText &&
                       options.SequenceEqual(other.options) &&
                       correctAnswerIndex == other.correctAnswerIndex;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (questionIndex + questionText + JsonConvert.SerializeObject(options) + correctAnswerIndex).GetHashCode();
        }
    }

    [Serializable]
    public class Option
    {
        public int optionIndex;
        public string optionText;
    }

    [Serializable]
    public class HistoryEntry
    {
        public List<QuizQuestion> questions;
        public string jsonSnapshot;
        public string timestamp;
        public List<string> deltaChanges;

        public HistoryEntry(List<QuizQuestion> q, string json, List<QuizQuestion> previousQuestions)
        {
            questions = new List<QuizQuestion>(q);
            jsonSnapshot = json;
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            deltaChanges = ComputeDelta(previousQuestions, q);
        }

        private List<string> ComputeDelta(List<QuizQuestion> prev, List<QuizQuestion> current)
        {
            var delta = new List<string>();
            prev ??= new List<QuizQuestion>();

            // Check for added or modified questions
            for (int i = 0; i < current.Count; i++)
            {
                var currentQuestion = current[i];
                var match = prev.Count > i ? prev[i] : null;

                if (match == null)
                {
                    // New question added
                    delta.Add($"+ Added Question {i}: \"{currentQuestion.questionText}\"");
                }
                else if (!currentQuestion.Equals(match))
                {
                    // Detailed modification tracking
                    if (currentQuestion.questionIndex != match.questionIndex)
                    {
                        delta.Add($"~ Question {i} Index Changed: From \"{match.questionIndex}\" to \"{currentQuestion.questionIndex}\"");
                    }
                    
                    if (currentQuestion.questionText != match.questionText)
                    {
                        delta.Add($"~ Question {i} Text Changed: From \"{match.questionText}\" to \"{currentQuestion.questionText}\"");
                    }

                    if (currentQuestion.correctAnswerIndex != match.correctAnswerIndex)
                    {
                        delta.Add($"~ Question {i} Correct Answer Index Changed: From {match.correctAnswerIndex} to {currentQuestion.correctAnswerIndex}");
                    }

                    // Track option changes
                    if (currentQuestion.options.Count != match.options.Count)
                    {
                        delta.Add($"~ Question {i} Options Count Changed: From {match.options.Count} to {currentQuestion.options.Count}");
                    }
                    else
                    {
                        // Check for specific option modifications
                        for (int j = 0; j < currentQuestion.options.Count; j++)
                        {
                            if (currentQuestion.options[j].optionText != match.options[j].optionText)
                            {
                                delta.Add($"~ Question {i} Option {j} Text Changed: From \"{match.options[j].optionText}\" to \"{currentQuestion.options[j].optionText}\"");
                            }
                        }
                    }
                }
            }

            // Check for removed questions
            if (prev.Count > current.Count)
            {
                for (int i = current.Count; i < prev.Count; i++)
                {
                    delta.Add($"- Removed Question {i}: \"{prev[i].questionText}\"");
                }
            }

            return delta;
        }
    }

    public List<QuizQuestion> Questions = new();
    public string JsonPreview = "[]";
    public string filePath;
    public string historyFilePath;
    public List<HistoryEntry> changeHistory = new();
    public int maxHistorySize = 100;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "quizData.json");
        historyFilePath = Path.Combine(Application.persistentDataPath, "quizHistory.json");

        LoadHistoryFromFile();

        if (File.Exists(filePath))
        {
            Questions = LoadQuestionsFromJson();
            if (changeHistory.Count == 0)
            {
                SyncJsonPreviewFromQuestions();
                AddToHistory(Questions);
            }
        }
    }

    public void SaveQuestionsToJson()
    {
        try
        {
            // Store the current state before modifying
            List<QuizQuestion> currentQuestions = new List<QuizQuestion>(Questions);

            string json = JsonConvert.SerializeObject(Questions, Formatting.Indented);
            File.WriteAllText(filePath, json);

            // Explicitly sync and add to history AFTER saving
            SyncJsonPreviewFromQuestions();

            // Add current state to history
            AddToHistory(currentQuestions);

            SaveHistoryToFile();
            Debug.Log("Questions saved to: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving JSON: " + e.Message);
        }
    }

    public List<QuizQuestion> LoadQuestionsFromJson()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                List<QuizQuestion> loadedQuestions = JsonConvert.DeserializeObject<List<QuizQuestion>>(json);
                SyncJsonPreviewFromQuestions();
                AddToHistory(loadedQuestions);
                SaveHistoryToFile();
                Debug.Log("Questions loaded successfully");
                return loadedQuestions;
            }
            else
            {
                Debug.LogWarning("JSON file not found at: " + filePath);
                return new List<QuizQuestion>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading JSON: " + e.Message);
            return new List<QuizQuestion>();
        }
    }

    public bool LoadQuestionsFromPreview()
    {
        try
        {
            List<QuizQuestion> loadedQuestions = JsonConvert.DeserializeObject<List<QuizQuestion>>(JsonPreview);
            if (loadedQuestions != null)
            {
                Questions = loadedQuestions;
                SyncJsonPreviewFromQuestions();
                AddToHistory(loadedQuestions);
                SaveHistoryToFile();
                Debug.Log("Questions loaded from preview successfully");
                return true;
            }
            else
            {
                Debug.LogWarning("Preview JSON is invalid or empty");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading from preview: " + e.Message);
            return false;
        }
    }

    public void SyncJsonPreviewFromQuestions()
    {
        JsonPreview = Questions.Count > 0
            ? JsonConvert.SerializeObject(Questions, Formatting.Indented)
            : "[]";
    }

    public QuizQuestion GetQuestion(int index)
    {
        if (index >= 0 && index < Questions.Count)
        {
            return Questions[index];
        }
        else
        {
            Debug.LogWarning("Invalid question index");
            return null;
        }
    }

    private void AddToHistory(List<QuizQuestion> currentQuestions)
    {
        string json = JsonConvert.SerializeObject(currentQuestions, Formatting.Indented);
        List<QuizQuestion> previousQuestions = changeHistory.Count > 0 ? changeHistory[0].questions : null;
        var newEntry = new HistoryEntry(currentQuestions, json, previousQuestions);

        if (newEntry.deltaChanges.Count > 0)
        {
            changeHistory.Insert(0, newEntry);
            if (changeHistory.Count > maxHistorySize)
            {
                changeHistory.RemoveAt(changeHistory.Count - 1);
            }
        }
    }

    public void RevertToHistory(int index)
    {
        if (index >= 0 && index < changeHistory.Count)
        {
            Questions = new List<QuizQuestion>(changeHistory[index].questions);
            JsonPreview = changeHistory[index].jsonSnapshot;
            AddToHistory(Questions);
            SaveHistoryToFile();
            Debug.Log($"Reverted to history state at {changeHistory[index].timestamp}");
        }
        else
        {
            Debug.LogWarning("Invalid history index");
        }
    }

    public void ResetHistory()
    {
        changeHistory.Clear();
        if (File.Exists(historyFilePath))
        {
            File.Delete(historyFilePath);
        }
        Debug.Log("History reset and file deleted.");
    }

    private void SaveHistoryToFile()
    {
        try
        {
            string json = JsonConvert.SerializeObject(changeHistory, Formatting.Indented);
            File.WriteAllText(historyFilePath, json);
            Debug.Log("History saved to: " + historyFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving history: " + e.Message);
        }
    }

    private void LoadHistoryFromFile()
    {
        try
        {
            if (File.Exists(historyFilePath))
            {
                string json = File.ReadAllText(historyFilePath);
                changeHistory = JsonConvert.DeserializeObject<List<HistoryEntry>>(json) ?? new List<HistoryEntry>();
                Debug.Log("History loaded from: " + historyFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading history: " + e.Message);
            changeHistory = new List<HistoryEntry>();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(QuizManager))]
public class QuizManagerEditor : Editor
{
    private Vector2 scrollPosition;
    private Vector2 historyScrollPosition;
    private Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();

    public override void OnInspectorGUI()
    {
        QuizManager quizManager = (QuizManager)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Questions"), true);

        EditorGUILayout.LabelField("JSON Preview", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        quizManager.JsonPreview = EditorGUILayout.TextArea(quizManager.JsonPreview, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("filePath"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("historyFilePath"), true);

        if (GUILayout.Button("Save to JSON"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Save",
                "Are you sure you want to save the current questions to the JSON file? This will overwrite the existing file.",
                "Yes",
                "No"))
            {
                quizManager.SaveQuestionsToJson();
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
        }

        if (GUILayout.Button("Load from JSON"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Load",
                "Are you sure you want to load questions from the JSON file? This will overwrite the current questions.",
                "Yes",
                "No"))
            {
                quizManager.Questions = quizManager.LoadQuestionsFromJson();
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
        }

        if (GUILayout.Button("Load from Preview"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Load from Preview",
                "Are you sure you want to overwrite the current questions with the data from the JSON preview?",
                "Yes",
                "No"))
            {
                if (quizManager.LoadQuestionsFromPreview())
                {
                    EditorUtility.DisplayDialog("Load Successful", "Questions loaded from preview.", "OK");
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Failed", "Check JSON format.", "OK");
                }
            }
        }

        EditorGUILayout.LabelField("Change History", EditorStyles.boldLabel);
        historyScrollPosition = EditorGUILayout.BeginScrollView(historyScrollPosition, GUILayout.Height(200));
        for (int i = 0; i < quizManager.changeHistory.Count; i++)
        {
            if (!foldoutStates.ContainsKey(i)) foldoutStates[i] = false;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], $"[{i}] {quizManager.changeHistory[i].timestamp}");
            if (foldoutStates[i])
            {
                EditorGUI.indentLevel++;
                foreach (var change in quizManager.changeHistory[i].deltaChanges)
                {
                    EditorGUILayout.LabelField(change);
                }
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Revert", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Revert",
                    $"Revert to state from {quizManager.changeHistory[i].timestamp}?",
                    "Yes",
                    "No"))
                {
                    quizManager.RevertToHistory(i);
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Reset History"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Reset History",
                "Are you sure you want to reset the change history? This will clear all history entries and delete the history file.",
                "Yes",
                "No"))
            {
                quizManager.ResetHistory();
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}
#endif
