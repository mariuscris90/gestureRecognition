   M S B A M L   `   `   `  ���� P  LDTWGestureRecognition, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:DTWGestureRecognitionDTWGestureRecognition  &     DTWGestureRecognition.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5local#clr-namespace:DTWGestureRecognition     -     � Title$  	KinectDTW��$	��759��$
��1298��.���  � RowDefinitions �� $��58 ��� $	��162 ��� $	��480 ��� $��16 ��� $��4* � � ColumnDefinitions �� $��81 ��� $	��119 ��� $	��440 ��� $
��636* �.+��� $��3��$��65,0,197,24q�$��1��.+��� $3��-/DTWGestureRecognition;component/HeaderBG.png��$	��713��$��35���� $��3��$��
0,40,407,0q�$��2��.+��� -   $��SelectOp��$��14(�$��0,0,0,0q�|�.  $��0q�$��Select Gesture to record:�� �� TextAlignment$
 Left=�$	��158��$��20���� -   $��dtwRead��$
��Read��$	��200��{�.  �� -   $��
dtwCapture��$��Capture��$
��200 ���� -   $��dtwStore��$��Store��{�.  $	��200���� -   $��dtwLoadFile��$��Load gesture file��{�. $	��200���� -   $��dtwSaveToFile��$��Save to file��{�. $	��200���� -   $��SliderValue��$��23��
�������$��Tilt angle: ��$	��201���� -	   $��
TiltSlider��$��23��$	��100�� � Minimum$	 -27Q� � Maximum$ 27Q��� $��2��$��1��$
��Left=�$	��401��$��39,0,0,0q�.+��� -
   $��gestureList��$	��400��.@��� .��
@Neutral�� .��@Yawn�� .��@Sad�� .��@Happy�� .��@ReiseEyeBrows�� .��@Head - Rotate to Left�� .��@Head - Rotate to Right�� .��@Head - Tremble�� .��@Right_hand_swipe_left�� .��@Right_hand_swipe_right�� .��@Left_hand_swipe_left�� .��@Left_hand_swipe_right�� .��@Two hands zoom in�� .��@Two hands zoom out�� .��@Right hand wave�� .��@Left_hand_wave�� .��@Right_hand_pull_down�� .��@Left_hand_pull_down�� .��@Right_hand_push_up�� .��@Left_hand_push_up�� .��@Both_hands_pull_down�� .��@Both_hands_push_up�� .��@Left_Hand_Up�� .��@Left_Hand_To_The_Left�� .��@Left_Hand_To_The_Right�� .��@Right_Hand_Up�� .��@Right_Hand_To_The_Left�� .��@Right_Hand_To_The_Right�� .��@Both_Hands_Up�� .��@Both_Hands_Down�� -   $��results��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Body - Gesture:��$
 Left=�$
��Left=�$	��Top=�$	��400��$��20���� -   $��resultsS��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Body - Static Gesture:��$
 Left=�$
��Left=�$	��Top=�$	��400��$��20���� -   $��faceResults��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Face - Gesture:��$
 Left=�$
��Left=�$	��Top=�$	��400��$��20���� -   $��faceResultsS��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Face - Static Gesture:��$
 Left=�$
��Left=�$	��Top=�$	��400��$��20���� -   $��status��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Status:��$
 Left=�$
��Left=�$	��Top=�$	��405��$��20���� -   $��currentBufferFrame��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Current Buffer Frame: ��$
 Left=�$
��Left=�$	��Top=�$	��404��$��20���� -   $��kinectStatus��$��14(�
�������$��0,0,0,0q�|�.  $��0q�$��Kinect: ��$
 Left=�$	��402��$��20��(�  (� Stretch$ Uniform=�$��2��$��3��.��� .+��� -   $��
videoImage��$	��640��$	��480���� -   $��skeletonCanvas��
������ �� $��3��$��4��$ ��By Giuroiu Marius-Cristian��
�������$	��Top=�$��Center=�$��16��$��10(�$��0q�����  �� EndPoint$ 0.5,1&� �� 
StartPoint$ 0.5,0&�.��� 	 � Color$	 	#FF4D35AB��
 � Offset$
 1Q�� $	 	#FF724798��$
 0.498Q�� $	 	#FF2B9E32��$
 0Q�(� $��0,0,-4,0q�$ Uniform=�$��3��$��2��.��� .+��� -   $��	FaceImage��$	��480��$	��640���� -   $��canvas1��
������ $��
20,18,12,6q�.   (DTWGestureRecognition.FaceTrackingViewer  -     Name$ faceTrackingViewer���� -   $��faceTracking��$��Enable Face��$��3��$��1��$��16��$
��Left=�$��
285,63,0,0q�$	��Top=�$	��165���� -   $��	enableNui��$#��Enable Natural User Interface��$��16��$��3��$��285,81,174,65q�$��1���� -   $��seated��$��Seated��$��3��$��1��$��16��$
��Left=�$��
285,97,0,0q�$	��Top=�$	��165���� -   $��nearTracking��$��Near tracking��$��3��$��1��$��16��$
��Left=�$��285,114,0,0q�$	��Top=��� -   $��faceOrientation��$��3��$��1��$��20��$
��Left=�$��	8,139,0,0q�$	��Top=�$	��366���� -   $��faceMesh��$��Disable Face Mesh��$��3��$��1��$
��Left=�$��285,44,0,102q�$	��123��	� -   $��captureBody��$��Body - Dynamic Gestures��$��3��$��1��$��16��$
��Left=�$��
468,11,0,0q�$	��Top=�	� -   $��captureBodyS��$��Body - Static Gestures��$��3��$��1��$��16��$
��Left=�$��
468,31,0,0q�$	��Top=�	� -   $��captureFace��$��Face - Dynamic Gestures��$��3��$��1��$��16��$
��Left=�$��
468,51,0,0q�$	��Top=�	� -    $��captureFaceS��$��Face - Static Gestures��$��3��$��1��$��16��$
��Left=�$��
468,71,0,0q�$	��Top=��� $��30��$��Gesture Recognition��$��14(�$
��Bold"�$	��600��$��SemiExpanded&�$��Normal$�$��Center=�$	��Top=�$��Segoe UI)�$��2��$��2��$��119,28,357,0q��� -!   $��label1��$��Recognized Speech:��$��3��$��1��$��28��$
��Left=�$��8,3,0,0q�$	��Top=�$	��122���� -"   $��RecognizedSpeech��$��3��$��1��
�������$��23��$
��Left=�$��	123,4,0,0q�$	��Top=�$	��156��|�.  $��0q�$��You said: ... ��	� -#   $��captureFaceO��$ ��Face - Dynamic Orientation��$��3��$��1��$��16��$
��Left=�$��
468,91,0,0q�$	��Top=�$	��170��	� -$   $��captureHandS��$��Hand - Static Gestures��$��3��$��1��$��16��$
��Left=�$��468,131,0,0q�$	��Top=�$	��158��	� -%   $��captureHand��$��Hand - Dynamic Gestures��$��3��$��1��$��16��$
��Left=�$��468,111,0,0q�$	��Top=�$	��158���� -&   $��handGestureText��$��3��
�������$��1��$��23��|�.  $��0q�$
��Left=�$��	10,55,0,0q�$��Hand - Gesture: ��$	��Top=�$	��269���� -'   $��handStaticGesture��$��3��
�������|�.  $��0q�$��1��$��23��$
��Left=�$��	10,30,0,0q�$��Hands : ��$	��Top=�$	��269���� -(   $��faceDynamicOrientation��$��3��
�������|�.  $��0q�$��1��$��23��$
��Left=�$��	10,87,0,0q�$!��Face - Dynamic Orientation:��$	��Top=�$	��269���� -)   $��faceOrientationText��$��3��
�������|�.  $��0q�$��1��$��23��$
��Left=�$��
10,112,0,0q�$��Face - Orientation:��$	��Top=�$	��269���� -*   $��label2��$��Enable/Disable Options��$��3��$��23��$
��Left=�$��
298,40,0,0q�$	��Top=�$	��141��$��2���� -+   $��label3��$��Record/Load Gestures��$��3��$��28��$
��Left=�$��
468,40,0,0q�$	��Top=�$	��131��$��2���� -,   $��enableHandTracking��$��Enable Hands Tracking ��$��3��$��1��$��16��$
��Left=�$��
285,25,0,0q�$	��Top=��� --   $��handContour��$��Enable Hand Contour��$��3��$��1��$
��Left=�$��285,6,0,140q�$	��144��