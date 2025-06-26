using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace CyberChatBot
{
    public partial class MainWindow : Window
    {
        private List<ChatMessage> chatHistory = new();
        private List<CyberTask> tasks = new();
        private List<ActivityLogEntry> activityLog = new();
        private List<QuizQuestion> quizQuestions = new();
        private List<int> highScores = new();

        private int currentQuestionIndex = 0;
        private int quizScore = 0;
        private bool quizInProgress = false;

        private Dictionary<string, List<string>> nlpKeywords = new()
        {
            { "task", new() { "task", "reminder", "todo", "add", "create", "set up", "enable", "configure" } },
            { "quiz", new() { "quiz", "test", "question", "game", "challenge" } },
            { "phishing", new() { "phishing", "scam", "email", "fake", "fraud" } },
            { "password", new() { "password", "login", "credential", "authentication", "2fa", "two-factor" } },
            { "malware", new() { "malware", "virus", "trojan", "ransomware", "spyware" } },
            { "browsing", new() { "browsing", "website", "internet", "web", "online" } },
            { "firewall", new() { "firewall", "network", "security", "protection" } },
            { "activity", new() { "activity", "log", "history", "what have you done", "actions" } }
        };

        private readonly string[] tips =
        {
            "💡 Use strong passwords with mixed characters!",
            "💡 Enable two-factor authentication where possible!",
            "💡 Keep your software updated regularly!",
            "💡 Be cautious with email attachments!",
            "💡 Use HTTPS websites for sensitive data!",
            "💡 Regular backups can save you from ransomware!",
            "💡 Don't click suspicious links in emails!"
        };

        private readonly Random random = new();
        private DispatcherTimer tipTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
            PlayGreeting();
        }

        private void PlayGreeting()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
            if (File.Exists(filePath))
            {
                SoundPlayer player = new(filePath);
                player.Play();
            }
            else
            {
                MessageBox.Show("Greeting sound file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeApplication()
        {
            string userName = Microsoft.VisualBasic.Interaction.InputBox("Welcome! What's your name?", "Cybersecurity Chatbot", "Guest");
            if (string.IsNullOrWhiteSpace(userName)) userName = "Guest";
            UserNameDisplay.Text = $"Welcome, {userName}!";

            InitializeQuizQuestions();
            StartTipRotation();

            AddChatMessage("Bot", $"👋 Hi {userName}! I'm your cybersecurity assistant.");
            AddChatMessage("Bot", "💬 What would you like to do today?\n\n" +
                "1️⃣ Start a Quiz\n" +
                "2️⃣ Set a Reminder\n" +
                "3️⃣ View Activity Log\n" +
                "4️⃣ Learn about Phishing, Passwords or Malware\n\n" +
                "👉 You can type something like:\n- 'start quiz'\n- 'remind me in 2 days to update antivirus'\n- 'show activity log'\n\n" +
                "Or just type **menu** to see this list again.");

            LogActivity($"User {userName} started session");

            ShowChat(null, null);
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions = new()
            {
                new QuizQuestion("What should you do if you receive an email asking for your password?",
                    new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" }, 2,
                    "Report phishing emails to help prevent scams and protect others."),
                new QuizQuestion("What makes a strong password?",
                    new[] { "Your name and birthday", "12345", "Mix of letters, numbers, and symbols", "Your pet's name" }, 2,
                    "Use a mix of characters for secure passwords."),
                new QuizQuestion("What does HTTPS indicate?",
                    new[] { "A secure website", "A hacker website", "A game server", "A slow website" }, 0,
                    "HTTPS means the site encrypts your data."),
                new QuizQuestion("Should you click on links in suspicious emails?",
                    new[] { "Yes", "No", "Only if curious", "Only on weekends" }, 1,
                    "Never click suspicious email links."),
                new QuizQuestion("What is malware?",
                    new[] { "A cookie", "Harmful software", "A firewall", "An antivirus" }, 1,
                    "Malware is malicious software."),
                new QuizQuestion("What is the purpose of a firewall?",
                    new[] { "To start fires", "To protect networks", "To speed up internet", "To store passwords" }, 1,
                    "Firewalls protect your network."),
                new QuizQuestion("What is 2FA?",
                    new[] { "Two passwords", "Extra security", "Two computers", "Two browsers" }, 1,
                    "2FA adds an extra layer of protection."),
                new QuizQuestion("How often should you update software?",
                    new[] { "Never", "Once a year", "Regularly", "Only when it breaks" }, 2,
                    "Keep software updated to fix vulnerabilities."),
                new QuizQuestion("What to do on public Wi-Fi?",
                    new[] { "Use for banking", "Avoid sensitive actions", "Share passwords", "Download apps" }, 1,
                    "Avoid sensitive activities on public Wi-Fi."),
                new QuizQuestion("What is social engineering?",
                    new[] { "Building bridges", "Manipulating people", "Social media", "Programming" }, 1,
                    "It's a tactic to trick people into giving info.")
            };
        }

        private void StartTipRotation()
        {
            tipTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            tipTimer.Tick += (s, e) => TipDisplay.Text = tips[random.Next(tips.Length)];
            tipTimer.Start();
        }

        private bool ContainsKeywords(string input, List<string> keywords)
        {
            return keywords.Any(k => input.Contains(k));
        }

        private void AddChatMessage(string sender, string message)
        {
            chatHistory.Add(new ChatMessage(sender, message));
            ChatDisplay.Text += $"{sender}: {message}\n\n";
            ChatDisplay.ScrollToEnd();
        }

        private void LogActivity(string description)
        {
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, Description = description });
        }

        private string ProcessNLPInput(string input)
        {
            input = input.Trim();

            // Preprocess and analyze the input
            var analysis = AnalyzeInput(input);

            // Handle based on detected intent and context
            switch (analysis.Intent)
            {
                case UserIntent.StartQuiz:
                    return HandleQuizRequest(analysis);

                case UserIntent.SetReminder:
                    return HandleReminderRequest(analysis);

                case UserIntent.ViewActivity:
                    return HandleActivityRequest(analysis);

                case UserIntent.LearnTopic:
                    return HandleLearningRequest(analysis);

                case UserIntent.GetHelp:
                    return HandleHelpRequest(analysis);

                case UserIntent.Greeting:
                    return HandleGreeting(analysis);

                case UserIntent.Goodbye:
                    return HandleGoodbye(analysis);

                case UserIntent.Affirmative:
                    return HandleAffirmative(analysis);

                case UserIntent.Negative:
                    return HandleNegative(analysis);

                default:
                    return HandleUnknownRequest(analysis);
            }
        }

        // Enhanced input analysis with sentence structure recognition
        private InputAnalysis AnalyzeInput(string input)
        {
            var analysis = new InputAnalysis
            {
                OriginalInput = input,
                CleanInput = CleanInput(input),
                Intent = UserIntent.Unknown,
                Entities = new Dictionary<string, string>(),
                Confidence = 0.0,
                Context = new List<string>()
            };

            // Tokenize and analyze sentence structure
            var tokens = TokenizeInput(analysis.CleanInput);
            var sentenceType = DetermineSentenceType(analysis.CleanInput);
            var keywords = ExtractKeywords(tokens);

            analysis.Tokens = tokens;
            analysis.SentenceType = sentenceType;
            analysis.Keywords = keywords;

            // Intent classification with confidence scoring
            analysis.Intent = ClassifyIntent(analysis);
            analysis.Entities = ExtractEntities(analysis);

            return analysis;
        }

        private string CleanInput(string input)
        {
            // Normalize input while preserving sentence structure
            input = input.Trim().ToLower();
            input = Regex.Replace(input, @"[^\w\s\d\-']", " "); // Keep apostrophes and hyphens
            input = Regex.Replace(input, @"\s+", " ");
            return input;
        }

        private List<string> TokenizeInput(string input)
        {
            // Smart tokenization that handles contractions and compound words
            var tokens = new List<string>();
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Handle contractions
                if (word.Contains("'"))
                {
                    var parts = word.Split('\'');
                    tokens.AddRange(parts.Where(p => !string.IsNullOrEmpty(p)));
                }
                else
                {
                    tokens.Add(word);
                }
            }

            return tokens;
        }

        private SentenceType DetermineSentenceType(string input)
        {
            if (input.Contains("?") || StartsWithQuestionWord(input))
                return SentenceType.Question;
            else if (input.Contains("!") || ContainsImperativeVerbs(input))
                return SentenceType.Command;
            else if (ContainsRequestWords(input))
                return SentenceType.Request;
            else
                return SentenceType.Statement;
        }

        private bool StartsWithQuestionWord(string input)
        {
            string[] questionWords = { "what", "when", "where", "why", "how", "who", "which", "can", "could", "would", "should", "do", "does", "did", "is", "are", "will" };
            return questionWords.Any(qw => input.StartsWith(qw + " "));
        }

        private bool ContainsImperativeVerbs(string input)
        {
            string[] imperatives = { "start", "begin", "show", "tell", "help", "remind", "set", "add", "create", "run", "open", "close", "stop" };
            var firstWord = input.Split(' ')[0];
            return imperatives.Contains(firstWord);
        }

        private bool ContainsRequestWords(string input)
        {
            string[] requestWords = { "please", "could you", "would you", "can you", "i would like", "i want", "i need", "i'd like" };
            return requestWords.Any(rw => input.Contains(rw));
        }

        private List<string> ExtractKeywords(List<string> tokens)
        {
            // Define semantic keyword groups
            var keywordGroups = new Dictionary<string, string[]>
            {
                ["quiz"] = new[] { "quiz", "test", "question", "questions", "game", "challenge", "exam", "assessment", "knowledge" },
                ["reminder"] = new[] { "remind", "reminder", "reminders", "alert", "notification", "notify", "schedule", "task", "todo" },
                ["activity"] = new[] { "activity", "log", "history", "activities", "record", "records", "what", "done", "did" },
                ["learning"] = new[] { "learn", "teach", "education", "information", "about", "explain", "help", "guide", "tutorial" },
                ["phishing"] = new[] { "phishing", "phish", "email", "scam", "fraud", "fake", "suspicious", "spam" },
                ["password"] = new[] { "password", "passwords", "login", "authentication", "credential", "credentials", "2fa", "auth" },
                ["malware"] = new[] { "malware", "virus", "trojan", "spyware", "ransomware", "antivirus", "security", "threat" },
                ["time"] = new[] { "day", "days", "week", "weeks", "month", "months", "year", "tomorrow", "today", "later" }
            };

            var foundKeywords = new List<string>();

            foreach (var group in keywordGroups)
            {
                if (tokens.Any(token => group.Value.Contains(token)))
                {
                    foundKeywords.Add(group.Key);
                }
            }

            return foundKeywords;
        }

        private UserIntent ClassifyIntent(InputAnalysis analysis)
        {
            double maxConfidence = 0.0;
            UserIntent bestIntent = UserIntent.Unknown;

            // Intent classification with confidence scoring
            var intentScores = new Dictionary<UserIntent, double>();

            // Quiz intent patterns
            if (analysis.Keywords.Contains("quiz") ||
                analysis.CleanInput.Contains("1") ||
                MatchesPattern(analysis.CleanInput, new[] { "start.*quiz", "take.*test", "begin.*game", "i want.*quiz", "quiz.*me", "test.*knowledge" }))
            {
                intentScores[UserIntent.StartQuiz] = CalculateConfidence(analysis, new[] { "quiz", "test", "start", "begin" });
            }

            // Reminder intent patterns
            if (analysis.Keywords.Contains("reminder") ||
                analysis.CleanInput.Contains("2") ||
                MatchesPattern(analysis.CleanInput, new[] { "remind.*me", "set.*reminder", "add.*task", "schedule.*task", "i need.*reminder", "don.*forget" }))
            {
                intentScores[UserIntent.SetReminder] = CalculateConfidence(analysis, new[] { "remind", "reminder", "task", "schedule" });
            }

            // Activity intent patterns
            if (analysis.Keywords.Contains("activity") ||
                analysis.CleanInput.Contains("3") ||
                MatchesPattern(analysis.CleanInput, new[] { "show.*activity", "view.*log", "what.*did", "my.*history", "activity.*log", "what.*done" }))
            {
                intentScores[UserIntent.ViewActivity] = CalculateConfidence(analysis, new[] { "activity", "log", "history", "show" });
            }

            // Learning intent patterns
            if (analysis.Keywords.Contains("learning") ||
                analysis.CleanInput.Contains("4") ||
                MatchesPattern(analysis.CleanInput, new[] { "learn.*about", "tell.*me.*about", "what.*is", "explain", "help.*me.*understand", "teach.*me" }))
            {
                intentScores[UserIntent.LearnTopic] = CalculateConfidence(analysis, new[] { "learn", "teach", "explain", "about" });
            }

            // Help intent patterns
            if (MatchesPattern(analysis.CleanInput, new[] { "help", "menu", "options", "what.*can.*you", "how.*do", "i.*don.*know", "commands" }))
            {
                intentScores[UserIntent.GetHelp] = CalculateConfidence(analysis, new[] { "help", "menu", "options" });
            }

            // Greeting patterns
            if (MatchesPattern(analysis.CleanInput, new[] { "hello", "hi", "hey", "good.*morning", "good.*afternoon", "good.*evening", "greetings" }))
            {
                intentScores[UserIntent.Greeting] = CalculateConfidence(analysis, new[] { "hello", "hi", "hey" });
            }

            // Goodbye patterns
            if (MatchesPattern(analysis.CleanInput, new[] { "bye", "goodbye", "see.*you", "exit", "quit", "thank.*you", "thanks" }))
            {
                intentScores[UserIntent.Goodbye] = CalculateConfidence(analysis, new[] { "bye", "goodbye", "thanks" });
            }

            // Affirmative patterns
            if (MatchesPattern(analysis.CleanInput, new[] { "yes", "yeah", "yep", "sure", "okay", "ok", "alright", "absolutely", "definitely" }))
            {
                intentScores[UserIntent.Affirmative] = CalculateConfidence(analysis, new[] { "yes", "sure", "ok" });
            }

            // Negative patterns
            if (MatchesPattern(analysis.CleanInput, new[] { "no", "nope", "not", "never", "cancel", "stop", "don.*want" }))
            {
                intentScores[UserIntent.Negative] = CalculateConfidence(analysis, new[] { "no", "not", "cancel" });
            }

            // Find the intent with highest confidence
            if (intentScores.Any())
            {
                var best = intentScores.OrderByDescending(kvp => kvp.Value).First();
                bestIntent = best.Key;
                maxConfidence = best.Value;
            }

            analysis.Confidence = maxConfidence;
            return bestIntent;
        }

        private bool MatchesPattern(string input, string[] patterns)
        {
            return patterns.Any(pattern => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }

        private double CalculateConfidence(InputAnalysis analysis, string[] relevantWords)
        {
            double confidence = 0.0;
            int totalWords = analysis.Tokens.Count;
            int matchedWords = analysis.Tokens.Count(t => relevantWords.Contains(t));

            // Base confidence from keyword matches
            confidence += (double)matchedWords / totalWords * 0.6;

            // Bonus for sentence type alignment
            if (analysis.SentenceType == SentenceType.Command || analysis.SentenceType == SentenceType.Request)
                confidence += 0.2;

            // Bonus for direct keyword matches
            foreach (var word in relevantWords)
            {
                if (analysis.CleanInput.Contains(word))
                    confidence += 0.1;
            }

            return Math.Min(confidence, 1.0);
        }

        private Dictionary<string, string> ExtractEntities(InputAnalysis analysis)
        {
            var entities = new Dictionary<string, string>();

            // Extract time entities
            var timeMatch = Regex.Match(analysis.CleanInput, @"(\d+)\s*(day|days|week|weeks|month|months)");
            if (timeMatch.Success)
            {
                entities["time_amount"] = timeMatch.Groups[1].Value;
                entities["time_unit"] = timeMatch.Groups[2].Value;
            }

            // Extract date entities
            var dateMatch = Regex.Match(analysis.CleanInput, @"(\d{4}-\d{2}-\d{2})");
            if (dateMatch.Success)
            {
                entities["specific_date"] = dateMatch.Groups[1].Value;
            }

            // Extract cybersecurity topics
            if (analysis.Keywords.Contains("phishing"))
                entities["topic"] = "phishing";
            else if (analysis.Keywords.Contains("password"))
                entities["topic"] = "password";
            else if (analysis.Keywords.Contains("malware"))
                entities["topic"] = "malware";

            // Extract task description for reminders
            var reminderMatch = Regex.Match(analysis.CleanInput, @"remind me (?:in \d+ \w+ )?to (.+)");
            if (reminderMatch.Success)
            {
                entities["task_description"] = reminderMatch.Groups[1].Value.Trim();
            }

            return entities;
        }

        // Intent-specific handlers with natural responses
        private string HandleQuizRequest(InputAnalysis analysis)
        {
            currentQuestionIndex = 0;
            quizScore = 0;
            quizInProgress = true;
            ShowQuiz(null, null);
            LogActivity("Quiz started via natural language");

            var responses = new[]
            {
        "🧠 Great! Let's test your cybersecurity knowledge!",
        "🎯 Perfect timing for a quiz! Let's see what you know!",
        "📚 Excellent choice! Time to challenge yourself!",
        "🚀 Ready to become a cybersecurity expert? Let's go!"
    };

            var randomResponse = responses[new Random().Next(responses.Length)];
            return randomResponse + "\n\n" + AskNextQuizQuestion();
        }

        private string HandleReminderRequest(InputAnalysis analysis)
        {
            if (analysis.CleanInput.StartsWith("remind me"))
            {
                DateTime dueDate = DateTime.Now.AddDays(3);
                string taskDescription = "General cybersecurity task";

                // Extract specific timing and task details
                if (analysis.Entities.ContainsKey("time_amount") && analysis.Entities.ContainsKey("time_unit"))
                {
                    int amount = int.Parse(analysis.Entities["time_amount"]);
                    string unit = analysis.Entities["time_unit"];

                    if (unit.Contains("day"))
                        dueDate = DateTime.Now.AddDays(amount);
                    else if (unit.Contains("week"))
                        dueDate = DateTime.Now.AddDays(amount * 7);
                    else if (unit.Contains("month"))
                        dueDate = DateTime.Now.AddDays(amount * 30);
                }

                if (analysis.Entities.ContainsKey("task_description"))
                {
                    taskDescription = analysis.Entities["task_description"];
                }

                tasks.Add(new CyberTask
                {
                    Title = analysis.OriginalInput,
                    Description = taskDescription,
                    DueDate = dueDate
                });

                LogActivity($"Smart reminder created: {taskDescription} - Due {dueDate:yyyy-MM-dd}");
                RefreshTasksList();

                return $"✅ Perfect! I've scheduled a reminder for you:\n" +
                       $"📋 Task: {taskDescription}\n" +
                       $"📅 Due: {dueDate:MMMM dd, yyyy}\n" +
                       $"I'll make sure you don't forget! 😊";
            }
            else
            {
                return "📝 I'd be happy to set a reminder for you! Try saying something like:\n" +
                       "• 'Remind me in 3 days to update my antivirus'\n" +
                       "• 'Remind me to check my password security'\n" +
                       "• 'Set a reminder to review firewall settings'\n\n" +
                       "Just tell me what you want to remember and when! 🗓️";
            }
        }

        private string HandleActivityRequest(InputAnalysis analysis)
        {
            ShowActivityLog(null, null);
            var recentActivities = activityLog.TakeLast(5).ToList();

            if (recentActivities.Any())
            {
                var response = "📊 Here's what you've been up to recently:\n\n";
                response += string.Join("\n", recentActivities.Select(a => $"🔸 {a.Description} ({a.Timestamp:MMM dd, HH:mm})"));
                response += "\n\n💡 Keep up the great work on your cybersecurity journey!";
                return response;
            }
            else
            {
                return "📊 Your activity log is empty right now.\n" +
                       "Start by taking a quiz or setting some reminders to build your cybersecurity habits! 🚀";
            }
        }

        private string HandleLearningRequest(InputAnalysis analysis)
        {
            if (analysis.Entities.ContainsKey("topic"))
            {
                switch (analysis.Entities["topic"])
                {
                    case "phishing":
                        return "🎣 **Phishing Attacks Explained**\n\n" +
                               "Phishing is like digital fishing - criminals cast fake emails as 'bait' to catch your personal information! 🐟\n\n" +
                               "⚠️ **Watch out for:**\n" +
                               "• Urgent messages demanding immediate action\n" +
                               "• Suspicious sender addresses\n" +
                               "• Requests for passwords or personal info\n" +
                               "• Links that don't match the supposed sender\n\n" +
                               "🛡️ **Stay safe:** Always verify before you trust, and when in doubt, don't click!";

                    case "password":
                        return "🔐 **Password Security Mastery**\n\n" +
                               "Your passwords are the keys to your digital kingdom! 👑\n\n" +
                               "✅ **Best practices:**\n" +
                               "• Use unique passwords for each account\n" +
                               "• Mix uppercase, lowercase, numbers, and symbols\n" +
                               "• Consider using a password manager\n" +
                               "• Enable two-factor authentication (2FA)\n\n" +
                               "💡 **Pro tip:** Think of a memorable sentence and use the first letters: 'I love pizza on Fridays in 2024!' becomes 'IlpoFi2024!'";

                    case "malware":
                        return "🦠 **Malware: The Digital Disease**\n\n" +
                               "Malware is malicious software designed to harm your computer or steal your data! 💻\n\n" +
                               "🔍 **Types include:**\n" +
                               "• Viruses (spread and replicate)\n" +
                               "• Trojans (disguised as legitimate software)\n" +
                               "• Ransomware (locks your files for money)\n" +
                               "• Spyware (secretly monitors your activity)\n\n" +
                               "🛡️ **Protection:** Keep antivirus updated, avoid suspicious downloads, and scan regularly!";
                }
            }

            return "📚 **Cybersecurity Learning Hub**\n\n" +
                   "I can teach you about these important topics:\n\n" +
                   "🎣 **Phishing** - Email scams and how to spot them\n" +
                   "🔐 **Passwords** - Creating and managing secure credentials\n" +
                   "🦠 **Malware** - Understanding digital threats\n\n" +
                   "Just ask me something like:\n" +
                   "• 'Tell me about phishing attacks'\n" +
                   "• 'How do I create strong passwords?'\n" +
                   "• 'What is malware and how do I protect myself?'\n\n" +
                   "Knowledge is your best defense! 🛡️";
        }

        private string HandleHelpRequest(InputAnalysis analysis)
        {
            return "🤖 **Your Cybersecurity Assistant**\n\n" +
                   "I understand natural language, so you can talk to me like a real person! Here's what I can do:\n\n" +
                   "🧠 **Take a Quiz** - Test your cybersecurity knowledge\n" +
                   "📝 **Set Reminders** - Never forget important security tasks\n" +
                   "📊 **View Activity** - See your learning progress\n" +
                   "📚 **Learn Topics** - Get expert cybersecurity advice\n\n" +
                   "**Try saying things like:**\n" +
                   "• 'I want to take a cybersecurity quiz'\n" +
                   "• 'Remind me in 5 days to update my passwords'\n" +
                   "• 'What have I done recently?'\n" +
                   "• 'Teach me about phishing emails'\n" +
                   "• 'Can you help me learn about online security?'\n\n" +
                   "I'm here to help you stay safe online! 🌐";
        }

        private string HandleGreeting(InputAnalysis analysis)
        {
            var greetings = new[]
            {
        "👋 Hello there! Ready to boost your cybersecurity knowledge today?",
        "🌟 Hi! Great to see you back! What cybersecurity topic interests you today?",
        "👋 Hey! I'm your friendly cybersecurity assistant. How can I help you stay safe online?",
        "🎉 Hello! Welcome to your personal cybersecurity learning hub!"
    };

            var greeting = greetings[new Random().Next(greetings.Length)];
            return greeting + "\n\nTry saying something like 'I want to learn about passwords' or 'start a quiz'!";
        }

        private string HandleGoodbye(InputAnalysis analysis)
        {
            var farewells = new[]
            {
        "👋 Goodbye! Remember to stay vigilant online!",
        "🛡️ Take care and keep those cyber defenses strong!",
        "🌟 Thanks for learning with me! Stay safe out there!",
        "👋 Until next time - keep your cybersecurity knowledge sharp!"
    };

            return farewells[new Random().Next(farewells.Length)];
        }

        private string HandleAffirmative(InputAnalysis analysis)
        {
            return "👍 Great! I'm ready to help. What would you like to do?";
        }

        private string HandleNegative(InputAnalysis analysis)
        {
            return "👌 No problem! Is there something else I can help you with instead?";
        }

        private string HandleUnknownRequest(InputAnalysis analysis)
        {
            // Intelligent fallback with suggestions based on partial understanding
            var suggestions = new List<string>();

            if (analysis.Keywords.Any(k => new[] { "quiz", "learning", "password", "phishing", "malware" }.Contains(k)))
            {
                suggestions.Add("🧠 Take a cybersecurity quiz");
                suggestions.Add("📚 Learn about online security");
            }

            if (analysis.Keywords.Contains("time") || analysis.Tokens.Any(t => Regex.IsMatch(t, @"\d+")))
            {
                suggestions.Add("📝 Set a security reminder");
                suggestions.Add("📅 Schedule a cybersecurity task");
            }

            if (analysis.SentenceType == SentenceType.Question)
            {
                suggestions.Add("❓ Ask me about cybersecurity topics");
                suggestions.Add("💬 Try asking 'What is phishing?' or 'How do I create strong passwords?'");
            }

            string response = "🤔 I want to help, but I'm not quite sure what you're looking for.\n\n";

            if (suggestions.Any())
            {
                response += "Based on what you said, you might want to:\n";
                response += string.Join("\n", suggestions.Take(3).Select(s => $"• {s}"));
                response += "\n\n";
            }

            response += "💡 **Try saying:**\n" +
                        "• 'Start a quiz' or 'Test my knowledge'\n" +
                        "• 'Remind me to update my antivirus'\n" +
                        "• 'Tell me about password security'\n" +
                        "• 'Help' or 'What can you do?'\n\n" +
                        "I understand natural language, so just talk to me normally! 😊";

            return response;
        }

        // Supporting data structures
        public enum UserIntent
        {
            Unknown,
            StartQuiz,
            SetReminder,
            ViewActivity,
            LearnTopic,
            GetHelp,
            Greeting,
            Goodbye,
            Affirmative,
            Negative
        }

        public enum SentenceType
        {
            Statement,
            Question,
            Command,
            Request
        }

        public class InputAnalysis
        {
            public string OriginalInput { get; set; }
            public string CleanInput { get; set; }
            public UserIntent Intent { get; set; }
            public SentenceType SentenceType { get; set; }
            public List<string> Tokens { get; set; }
            public List<string> Keywords { get; set; }
            public Dictionary<string, string> Entities { get; set; }
            public double Confidence { get; set; }
            public List<string> Context { get; set; }
        }



        private string AskNextQuizQuestion()
        {
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                quizInProgress = false;
                string result = $"Quiz complete! You scored {quizScore} out of {quizQuestions.Count}. {(quizScore >= 7 ? "🎉 Great job!" : "Keep practicing!")}";
                LogActivity($"Quiz completed with score {quizScore}");
                QuizStartButton.Visibility = Visibility.Visible;
                QuizQuestionText.Text = "";
                QuizOptionsPanel.Children.Clear();

                // Save high score
                highScores.Add(quizScore);

                AddChatMessage("Bot", result);
                return "";
            }

            var q = quizQuestions[currentQuestionIndex];
            QuizQuestionText.Text = q.Question;
            QuizOptionsPanel.Children.Clear();

            for (int i = 0; i < q.Options.Length; i++)
            {
                var optionBtn = new Button
                {
                    Content = $"{(char)('A' + i)}) {q.Options[i]}",
                    Tag = i,
                    Margin = new Thickness(0, 5, 0, 0),
                    Background = Brushes.DarkSlateBlue,
                    Foreground = Brushes.White,
                    FontSize = 14
                };
                optionBtn.Click += QuizOption_Click;
                QuizOptionsPanel.Children.Add(optionBtn);
            }

            QuizFeedbackText.Text = "";
            QuizStartButton.Visibility = Visibility.Collapsed;

            return "";
        }

        private void QuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (!quizInProgress) return;

            var btn = (Button)sender;
            int selected = (int)btn.Tag;
            var q = quizQuestions[currentQuestionIndex];

            if (selected == q.CorrectAnswer)
            {
                quizScore++;
                QuizFeedbackText.Foreground = Brushes.LightGreen;
                QuizFeedbackText.Text = "Correct! " + q.Explanation;
                AddChatMessage("Bot", $"Correct! {q.Explanation}");
            }
            else
            {
                QuizFeedbackText.Foreground = Brushes.OrangeRed;
                QuizFeedbackText.Text = $"Incorrect. {q.Explanation}";
                AddChatMessage("Bot", $"Incorrect. {q.Explanation}");
            }

            currentQuestionIndex++;

            // Delay showing next question for 2 sec
            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(2000);
                AskNextQuizQuestion();
            });
        }

        private string ProcessQuizAnswer(string input)
        {
            // Accept answers as letters A-D or numbers 1-4 (case insensitive)
            string trimmed = input.Trim().ToUpper();
            int answerIndex = -1;

            if (trimmed.Length == 1)
            {
                char c = trimmed[0];
                if (c >= 'A' && c <= 'D') answerIndex = c - 'A';
                else if (c >= '1' && c <= '4') answerIndex = c - '1';
            }

            if (answerIndex == -1)
            {
                return "Please answer with A, B, C, or D.";
            }

            var q = quizQuestions[currentQuestionIndex];

            if (answerIndex == q.CorrectAnswer)
            {
                quizScore++;
                AddChatMessage("Bot", "Correct!");
            }
            else
            {
                AddChatMessage("Bot", $"Incorrect. The correct answer was {(char)('A' + q.CorrectAnswer)}.");
            }

            currentQuestionIndex++;
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                quizInProgress = false;
                LogActivity($"Quiz completed with score {quizScore}");
                return $"Quiz complete! Your score: {quizScore}/{quizQuestions.Count}.";
            }
            else
            {
                return AskNextQuizQuestion();
            }
        }

        private void RefreshTasksList()
        {
            TaskListBox.Items.Clear();
            foreach (var task in tasks.OrderBy(t => t.DueDate))
            {
                TaskListBox.Items.Add($"{task.Title} - Due {task.DueDate:yyyy-MM-dd}");
            }
        }

        private void ShowChat(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = Visibility.Visible;
            TasksPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Collapsed;
            ActivityPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowTasks(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Visible;
            QuizPanel.Visibility = Visibility.Collapsed;
            ActivityPanel.Visibility = Visibility.Collapsed;

            RefreshTasksList();
        }

        private void ShowQuiz(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Visible;
            ActivityPanel.Visibility = Visibility.Collapsed;

            QuizStartButton.Visibility = Visibility.Visible;
            QuizQuestionText.Text = "";
            QuizOptionsPanel.Children.Clear();
            QuizFeedbackText.Text = "";
        }

        private void ShowActivityLog(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Collapsed;
            ActivityPanel.Visibility = Visibility.Visible;

            ActivityDisplay.Text = string.Join(Environment.NewLine, activityLog.Select(a => $"{a.Timestamp:G}: {a.Description}"));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserMessage();
                e.Handled = true;
            }
        }

        private void ProcessUserMessage()
        {
            string userInput = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            AddChatMessage("You", userInput);
            MessageInput.Clear();

            string botResponse = ProcessNLPInput(userInput);
            if (!string.IsNullOrEmpty(botResponse))
                AddChatMessage("Bot", botResponse);
        }

        private void QuizStartButton_Click(object sender, RoutedEventArgs e)
        {
            quizInProgress = true;
            quizScore = 0;
            currentQuestionIndex = 0;
            AddChatMessage("Bot", "Starting the quiz! Good luck!");
            AskNextQuizQuestion();
        }
    }

    public record ChatMessage(string Sender, string Message);

    public class CyberTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }

    public class QuizQuestion
    {
        public string Question { get; }
        public string[] Options { get; }
        public int CorrectAnswer { get; }
        public string Explanation { get; }

        public QuizQuestion(string question, string[] options, int correctAnswer, string explanation)
        {
            Question = question;
            Options = options;
            CorrectAnswer = correctAnswer;
            Explanation = explanation;
        }
    }
}
