   M S B A M L   `   `   `  ����   CyberChatBot    CyberChatBot.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Cybersecurity Chatbot��$	��700��$	��900�����#1e1e2f
�������$��Segoe UI)� � 	Resources    RoundedButtonStyle �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius �� Triggers P� Property	 P� Value  TaskContextMenu  TaskItemStyle��%�  &	        &	 M    &	 �    7  �� " ��.���  � �� �#374a8a��  � ��
 �������  � ��$ SemiBold"���  � ��$	 8,4q���  � ��$ 5q���  � ��$	 100����  � ��$ 32����  � ��$ 0q���  �@�Cursor$
 Hand����  � ��$ Center=���  � ��$ Center=���  � �� �� " ��.R��� #��z��$ 15��#��z��#��z��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
	 True��. ���  � �� �#4c60c3P�  � ��$
	 True��. ���  � �� �#2e3a72P�  � {�$	 False��. ���  � �� �#888��  � �� �#ccc�� .@�m� -   $��Update Task��m� -   $��Delete Task���� " ��.���  �@�ContextMenu8   .��� $��10q�
 � RowDefinitions
 �� $
��Auto ��� $��40 ��� $��* ��� $	��100 ��� $��30 �.+��� -    ��Name$ UserNameDisplay��$��18(�$
��Bold"�$��0��$��5q���  �� Orientation$ 
Horizontal=�$��1��$��Center=�$
��0,10q�.+��� -   #��[  $
��Chat���� -   #��[  $��Tasks���� -   #��[  $
��Quiz���� -   #��[  $��Activity Log��$	��120��� $��2��.+�� -    �Name$ 	ChatPanel��$y�Visible=�.+��� -	    ��Name$ ChatDisplay��$��5q� �� 
IsReadOnly$
 True�� �� TextWrapping$
 Wrap=�" �� VerticalScrollBarVisibility$
 Auto=�$��14(�� -
   $ 
TasksPanel��$y�	Collapsed=�$��5q�.+��� -    ��Name$ TaskListBox��$	��350�����#2c2c44
�������$��14(�#��[ � -   $ 	QuizPanel��$y�	Collapsed=�$��5q�.+��� .+��� -   $ QuizQuestionText��$��16(�$
��Wrap=�$��0,0,0,10q��� -    ��Name$ QuizOptionsPanel���� -   $ QuizFeedbackText��$��14(�
������$��0,10,0,0q�$
��Wrap=��� -    ��Name$ QuizStartButton��#��[  $��
Start Quiz��$	��120��$��0,10,0,0q�� -   $ ActivityPanel��$y�	Collapsed=�.+��� -   $ ActivityDisplay��$��5q�$
 True��$
 Wrap=�$
 Auto=�$��14(�]� $��3��$��5q�.+��� -   $ MessageInput��$��0,0,5,0q�$��Center=�$	��750��$��14(��� -   #��[  $
��Send��$	��100���� -   $ 
TipDisplay��$��4��$��14(�$��Italic$�$��5,0,5,0q�
����ح�$
��Wrap=�