﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MarkingMenu
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        String log = "";

        int depth = 2;  // 2: 4*4, 3: 4*4*4        
        int participantNumber = 0; // 0 ~ 15, key for which random series
        int maxSession = 10;
        int numMenuPerParticipant = 8;
        int numDuplcationInSession = 2;

        int state;
        const int NOTRUNNING = -1, DEFAULT = 0, INVOCATE = 1, MARKING1 = 2, MARKING2 = 3, MARKING3 = 4;

        int screenWidth = 1366, screenHeight = 768;
        int rectWidth, rectHeight, rectNx, rectNy, rectN;

        Menus menus;
        Tasks tasks;

        Rectangle field;
        Rectangle invocation;
        TextBlock participantTextBlock;
        TextBlock taskTextBlock;

        Ellipse[] menuEllipse, submenuEllipse, subsubmenuEllipse;
        TextBlock[] menuTextBlock, submenuTextBlock, subsubmenuTextBlock;
        int currentMenu, currentSubmenu, currentSubsubmenu;

        TouchLine touchLine;

        SolidColorBrush buttonDownBrush, invocationUpBrush, invocationDownBrush, grayBrush, whiteBrush, blackBrush, lightgrayBrush, lightlightgrayBrush, darkgrayBrush;

        int[] sessionTasks;
        int currentTask;
        int[] markingState;

        public MainWindow()
        {
            InitializeComponent();

            state = NOTRUNNING;
            menus = new Menus(depth);
            tasks = new Tasks(depth, maxSession, numMenuPerParticipant, numDuplcationInSession, participantNumber);
            markingState = new int[depth];

            initializeSize();
            initializeBrushs();
            initializeField();
            initializeInvocation();
            initializePanel();
            initializeEllipse();
            initializeTouchLine();
        }

        private void initializeSize()
        {
            if (depth == 2)
            {
                rectWidth = 170; rectHeight = 152; rectNx = 4; rectNy = 4; rectN = rectNx * rectNy;
            }
            else
            {
                rectWidth = 85; rectHeight = 76; rectNx = 8; rectNy = 8; rectN = rectNx * rectNy;
            }
        }

        private void initializeBrushs()
        {
            buttonDownBrush = new SolidColorBrush(Color.FromRgb(230, 190, 190));
            invocationUpBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            invocationDownBrush = new SolidColorBrush(Color.FromRgb(150, 150, 30));
            grayBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            whiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            blackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            lightgrayBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            lightlightgrayBrush = new SolidColorBrush(Color.FromRgb(247, 247, 247));
            
        }

        private void initializeField()
        {
            field = new Rectangle();
            
            field.BeginInit();
            field.Width = screenWidth/2;
            field.Height = screenHeight;
            field.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            field.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            field.Fill = whiteBrush;
            field.Margin = new Thickness(screenWidth/2 , 0, 0, 0);
            field.EndInit();
            grid.Children.Add(field);

            field.MouseDown += new MouseButtonEventHandler(fieldDownHandler);
            field.MouseUp += new MouseButtonEventHandler(fieldUpHandler);
            field.MouseMove += new MouseEventHandler(fieldMoveHandler);

            field.TouchDown += new EventHandler<TouchEventArgs>(fieldDownHandler);
            field.TouchUp += new EventHandler<TouchEventArgs>(fieldUpHandler);
            field.TouchMove += new EventHandler<TouchEventArgs>(fieldMoveHandler);
            
        }

        private void initializeEllipse()
        {
            menuEllipse = new Ellipse[4];
            submenuEllipse = new Ellipse[4];
            subsubmenuEllipse = new Ellipse[4];
            menuTextBlock = new TextBlock[4];
            submenuTextBlock = new TextBlock[4];
            subsubmenuTextBlock = new TextBlock[4];

            int width = 100, height = 100;

            for (int i = 0; i < 4; i++)
            {
                menuEllipse[i] = new Ellipse();
                menuEllipse[i].BeginInit();
                menuEllipse[i].Width = width;
                menuEllipse[i].Height = height;
                menuEllipse[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                menuEllipse[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                menuEllipse[i].Visibility = System.Windows.Visibility.Hidden;
                menuEllipse[i].EndInit();
                grid.Children.Add(menuEllipse[i]);
                menuEllipse[i].TouchEnter += new EventHandler<TouchEventArgs>(menuEnterHandler);
                menuEllipse[i].MouseEnter += new MouseEventHandler(menuEnterHandler);
                menuEllipse[i].TouchMove += new EventHandler<TouchEventArgs>(fieldMoveHandler);
                menuEllipse[i].MouseMove += new MouseEventHandler(fieldMoveHandler);

                menuTextBlock[i] = new TextBlock();
                menuTextBlock[i].BeginInit();
                menuTextBlock[i].Width = width * 0.8;
                menuTextBlock[i].Height = 20;
                menuTextBlock[i].FontSize = 15;
                menuTextBlock[i].Padding = new Thickness(0, 0, 0, 0);
                menuTextBlock[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                menuTextBlock[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                menuTextBlock[i].TextAlignment = TextAlignment.Center;
                menuTextBlock[i].Visibility = System.Windows.Visibility.Hidden;
                menuTextBlock[i].EndInit();
                grid.Children.Add(menuTextBlock[i]);
                menuTextBlock[i].IsEnabled = false;

            }

            for (int i = 0; i < 4; i++)
            {
                submenuEllipse[i] = new Ellipse();
                submenuEllipse[i].BeginInit();
                submenuEllipse[i].Width = 100;
                submenuEllipse[i].Height = 100;
                submenuEllipse[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                submenuEllipse[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                submenuEllipse[i].Visibility = System.Windows.Visibility.Hidden;
                submenuEllipse[i].EndInit();
                grid.Children.Add(submenuEllipse[i]);
                submenuEllipse[i].TouchEnter += new EventHandler<TouchEventArgs>(submenuEnterHandler);
                submenuEllipse[i].MouseEnter += new MouseEventHandler(submenuEnterHandler);
                submenuEllipse[i].TouchMove += new EventHandler<TouchEventArgs>(fieldMoveHandler);
                submenuEllipse[i].MouseMove += new MouseEventHandler(fieldMoveHandler);
                submenuEllipse[i].TouchLeave += new EventHandler<TouchEventArgs>(submenuLeaveHandler);
                submenuEllipse[i].MouseLeave += new MouseEventHandler(submenuLeaveHandler);
                submenuEllipse[i].MouseUp += new MouseButtonEventHandler(submenuUpHandler);
                submenuEllipse[i].TouchUp += new EventHandler<TouchEventArgs>(submenuUpHandler);

                submenuTextBlock[i] = new TextBlock();
                submenuTextBlock[i].BeginInit();
                submenuTextBlock[i].Width = width * 0.8;
                submenuTextBlock[i].Height = 20;
                submenuTextBlock[i].FontSize = 15;
                submenuTextBlock[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                submenuTextBlock[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                submenuTextBlock[i].TextAlignment = TextAlignment.Center;
                submenuTextBlock[i].Background = grayBrush;
                submenuTextBlock[i].Foreground = blackBrush;
                submenuTextBlock[i].Visibility = System.Windows.Visibility.Hidden;
                submenuTextBlock[i].EndInit();
                grid.Children.Add(submenuTextBlock[i]);
                submenuTextBlock[i].TouchMove += new EventHandler<TouchEventArgs>(fieldMoveHandler);
                submenuTextBlock[i].MouseMove += new MouseEventHandler(fieldMoveHandler);
                submenuTextBlock[i].TouchEnter += new EventHandler<TouchEventArgs>(submenuTextBlockEnterHandler);
                submenuTextBlock[i].MouseEnter += new MouseEventHandler(submenuTextBlockEnterHandler);
                submenuTextBlock[i].MouseUp += new MouseButtonEventHandler(submenuUpHandler);
                submenuTextBlock[i].TouchUp += new EventHandler<TouchEventArgs>(submenuUpHandler);
            }
            for (int i = 0; i < 4; i++)
            {
                subsubmenuEllipse[i] = new Ellipse();
                subsubmenuEllipse[i].BeginInit();
                subsubmenuEllipse[i].Width = 100;
                subsubmenuEllipse[i].Height = 100;
                subsubmenuEllipse[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                subsubmenuEllipse[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                subsubmenuEllipse[i].Stroke = grayBrush;
                subsubmenuEllipse[i].Fill = grayBrush;
                subsubmenuEllipse[i].Visibility = System.Windows.Visibility.Hidden;
                subsubmenuEllipse[i].EndInit();
                grid.Children.Add(subsubmenuEllipse[i]);
                subsubmenuEllipse[i].TouchMove += new EventHandler<TouchEventArgs>(fieldMoveHandler);
                subsubmenuEllipse[i].MouseMove += new MouseEventHandler(fieldMoveHandler);
                subsubmenuEllipse[i].TouchEnter += new EventHandler<TouchEventArgs>(subsubmenuEnterHandler);
                subsubmenuEllipse[i].MouseEnter += new MouseEventHandler(subsubmenuEnterHandler);
                subsubmenuEllipse[i].TouchLeave += new EventHandler<TouchEventArgs>(subsubmenuLeaveHandler);
                subsubmenuEllipse[i].MouseLeave += new MouseEventHandler(subsubmenuLeaveHandler);
                subsubmenuEllipse[i].MouseUp += new MouseButtonEventHandler(subsubmenuUpHandler);
                subsubmenuEllipse[i].TouchUp += new EventHandler<TouchEventArgs>(subsubmenuUpHandler);

                subsubmenuTextBlock[i] = new TextBlock();
                subsubmenuTextBlock[i].BeginInit();
                subsubmenuTextBlock[i].Width = width * 0.8;
                subsubmenuTextBlock[i].Height = 20;
                subsubmenuTextBlock[i].FontSize = 15;
                subsubmenuTextBlock[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                subsubmenuTextBlock[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                subsubmenuTextBlock[i].TextAlignment = TextAlignment.Center;
                subsubmenuTextBlock[i].Background = grayBrush;
                subsubmenuTextBlock[i].Foreground = blackBrush;
                subsubmenuTextBlock[i].Visibility = System.Windows.Visibility.Hidden;
                subsubmenuTextBlock[i].EndInit();
                grid.Children.Add(subsubmenuTextBlock[i]);
                subsubmenuTextBlock[i].TouchMove += new EventHandler<TouchEventArgs>(fieldMoveHandler);
                subsubmenuTextBlock[i].MouseMove += new MouseEventHandler(fieldMoveHandler);
                subsubmenuTextBlock[i].TouchEnter += new EventHandler<TouchEventArgs>(subsubmenuTextBlockEnterHandler);
                subsubmenuTextBlock[i].MouseEnter += new MouseEventHandler(subsubmenuTextBlockEnterHandler);
                subsubmenuTextBlock[i].MouseUp += new MouseButtonEventHandler(subsubmenuUpHandler);
                subsubmenuTextBlock[i].TouchUp += new EventHandler<TouchEventArgs>(subsubmenuUpHandler);
            }
        }

        private void initializeInvocation()
        {
            invocation = new Rectangle();
            invocation.BeginInit();
            invocation.Width = 170; //rectWidth;
            invocation.Height = screenHeight - rectNy * rectHeight;
            invocation.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            invocation.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            invocation.Margin = new Thickness((screenWidth - rectNx * rectWidth), rectNy * rectHeight, 0, 0);
            invocation.Stroke = grayBrush;
            invocation.Fill = invocationUpBrush;
            invocation.Visibility = System.Windows.Visibility.Hidden;
            invocation.EndInit();
            grid.Children.Add(invocation);

            invocation.MouseLeftButtonDown += new MouseButtonEventHandler(invocateHandler);
            //invocation.MouseLeave += new MouseEventHandler(cancelInvocationHandler);
            //invocation.MouseLeftButtonUp += new MouseButtonEventHandler(cancelInvocationHandler);
            invocation.MouseRightButtonDown += new MouseButtonEventHandler(InvocationUpHandler);

            invocation.TouchDown += new EventHandler<TouchEventArgs>(invocateHandler);
            invocation.TouchEnter += new EventHandler<TouchEventArgs>(invocateHandler);
            invocation.TouchUp += new EventHandler<TouchEventArgs>(InvocationUpHandler);
            invocation.TouchLeave += new EventHandler<TouchEventArgs>(InvocationUpHandler);


        }
                
        void invocateHandler(object sender, RoutedEventArgs e)
        {
            if (state != DEFAULT)
                return;
            state = INVOCATE;
            invocation.Fill = invocationDownBrush;
        }
                
        void InvocationUpHandler(object sender, RoutedEventArgs e)
        {
            invocationUp();
        }
        void invocationUp()
        {
            state = DEFAULT;
            invocation.Fill = invocationUpBrush;         
            for(int i = 0; i< 4; i++){
                menuEllipse[i].Visibility = System.Windows.Visibility.Hidden;
                menuTextBlock[i].Visibility = System.Windows.Visibility.Hidden;
                submenuEllipse[i].Visibility = System.Windows.Visibility.Hidden;
                submenuTextBlock[i].Visibility = System.Windows.Visibility.Hidden;
                subsubmenuEllipse[i].Visibility = System.Windows.Visibility.Hidden;
                subsubmenuTextBlock[i].Visibility = System.Windows.Visibility.Hidden;
            }
            touchLine.hideLine();
        }

        void fieldDownHandler(object sender, MouseEventArgs e)
        {
            if (state != INVOCATE)
                return;
            Point point = e.GetPosition(grid);
            fieldDown(point.X, point.Y);            
        }
        void fieldDownHandler(object sender, TouchEventArgs e)
        {
            if (state != INVOCATE)
                return;
            Point point = e.GetTouchPoint(grid).Position;
            fieldDown(point.X, point.Y);
        }
        void fieldDown(double x, double y)
        {
            if (state != INVOCATE)
                return;
            state = MARKING1;

            int threshold1 = 120;
            menuEllipse[0].Margin = new Thickness(-menuEllipse[0].Width / 2 + x, y - threshold1 - menuEllipse[0].Height / 2, 0, 0);
            menuEllipse[1].Margin = new Thickness(-menuEllipse[1].Width / 2 + x + threshold1, -menuEllipse[1].Height / 2 + y, 0, 0);
            menuEllipse[2].Margin = new Thickness(-menuEllipse[2].Width / 2 +  x, y + threshold1 - menuEllipse[2].Height / 2, 0, 0);
            menuEllipse[3].Margin = new Thickness(-menuEllipse[3].Width / 2 +  x - threshold1, -menuEllipse[3].Height / 2 + y, 0, 0);

            menuTextBlock[0].Margin = new Thickness(-menuTextBlock[0].Width / 2 +  x, y - threshold1 - menuTextBlock[0].Height / 2, 0, 0);
            menuTextBlock[1].Margin = new Thickness(-menuTextBlock[1].Width / 2 +  x + threshold1, -menuTextBlock[1].Height / 2 + y, 0, 0);
            menuTextBlock[2].Margin = new Thickness(-menuTextBlock[2].Width / 2 + x, y + threshold1 - menuTextBlock[2].Height / 2, 0, 0);
            menuTextBlock[3].Margin = new Thickness(-menuTextBlock[3].Width / 2 + x - threshold1, -menuTextBlock[3].Height / 2 + y, 0, 0);

            touchLine.setXY(0, x, y);

            for (int i = 0; i < 4; i++)
            {
                menuEllipse[i].Fill = grayBrush;
                menuEllipse[i].Stroke = grayBrush;
                menuTextBlock[i].Background = grayBrush;
                menuTextBlock[i].Foreground = blackBrush;
                menuEllipse[i].Visibility = System.Windows.Visibility.Visible;
                menuTextBlock[i].Visibility = System.Windows.Visibility.Visible;
                if (depth == 2)
                    menuTextBlock[i].Text = menus.submenus[0,i];
                else
                    menuTextBlock[i].Text = menus.menus[i];
            }
        }

        void fieldMoveHandler(object sender, MouseEventArgs e){
            fieldMove(e.GetPosition(grid).X, e.GetPosition(grid).Y);
        }
        void fieldMoveHandler(object sender, TouchEventArgs e){
            fieldMove(e.GetTouchPoint(grid).Position.X, e.GetTouchPoint(grid).Position.Y);
        }
        void fieldMove(double x, double y){
            switch (state)
            {
                case DEFAULT:
                    touchLine.setXY(0, x, y);
                    break;
                case MARKING1:
                    touchLine.setXY(1, x, y);
                    touchLine.drawLine(1);
                    break;
                case MARKING2:
                    touchLine.setXY(2, x, y);
                    touchLine.drawLine(2);
                    break;
                case MARKING3:
                    touchLine.setXY(3, x, y);
                    touchLine.drawLine(3);
                    break;
                default:
                    break;
            }
        }

        void menuEnterHandler(object sender, RoutedEventArgs e)
        {
            if (state != MARKING1)
                return;
            state = MARKING2;

            for(int i =0; i<4; i++){
                if ((Ellipse)sender == menuEllipse[i])
                {
                    currentMenu = i;
                    touchLine.setXY(1, menuEllipse[i].Margin.Left + menuEllipse[i].Width / 2, menuEllipse[i].Margin.Top + menuEllipse[i].Height / 2);
                }
            }
            
            menuEnter(((Ellipse)sender).Margin.Left, ((Ellipse)sender).Margin.Top);
            
        }
        void menuEnter(double x, double y)
        {

            int threshold1 = 120;
            submenuEllipse[0].Margin = new Thickness(x, y - threshold1, 0, 0);
            submenuEllipse[1].Margin = new Thickness(x + threshold1, y, 0, 0);
            submenuEllipse[2].Margin = new Thickness(x, y + threshold1, 0, 0);
            submenuEllipse[3].Margin = new Thickness(x - threshold1, y, 0, 0);

            double x_ = (submenuEllipse[0].Width - submenuTextBlock[0].Width) / 2, y_ = (submenuEllipse[0].Height - submenuTextBlock[0].Height) / 2;
            submenuTextBlock[0].Margin = new Thickness(x + x_, y + y_ - threshold1, 0, 0);
            submenuTextBlock[1].Margin = new Thickness(x + x_ + threshold1, y + y_, 0, 0);
            submenuTextBlock[2].Margin = new Thickness(x + x_, y + y_ + threshold1, 0, 0);
            submenuTextBlock[3].Margin = new Thickness(x + x_ - threshold1, y + y_, 0, 0);

            for (int i = 0; i < 4; i++) {
                menuEllipse[i].Fill = lightgrayBrush;
                menuEllipse[i].Stroke = lightgrayBrush;
                menuTextBlock[i].Background = lightgrayBrush;
                menuTextBlock[i].Foreground = grayBrush;
                submenuEllipse[i].Fill = grayBrush;
                submenuEllipse[i].Stroke = grayBrush;
                submenuTextBlock[i].Background = grayBrush;
                submenuTextBlock[i].Foreground = blackBrush;
                submenuEllipse[i].Visibility = System.Windows.Visibility.Visible;
                submenuTextBlock[i].Visibility = System.Windows.Visibility.Visible;
                if (depth == 2)
                    submenuTextBlock[i].Text = menus.subsubmenus[0, currentMenu, i];
                else
                    submenuTextBlock[i].Text = menus.submenus[currentMenu, i];
            }
        }

        void submenuEnterHandler(object sender, RoutedEventArgs e)
        {
            if (state != MARKING2)
                return;
            switch(depth){
                case 2:
                    for (int i = 0; i < 4; i++)
                    {
                        if ((Ellipse)sender == submenuEllipse[i])
                        {
                            currentSubmenu = i;
                        }
                    }
                    submenuEnter();
                    break;
                case 3:
                    state = MARKING3;

                    for (int i = 0; i < 4; i++)
                    {
                        if ((Ellipse)sender == submenuEllipse[i])
                        {
                            currentSubmenu = i;
                            touchLine.setXY(2, submenuEllipse[i].Margin.Left + submenuEllipse[i].Width / 2, submenuEllipse[i].Margin.Top + submenuEllipse[i].Height / 2);
                        }
                    }
                    submenuEnter(((Ellipse)sender).Margin.Left, ((Ellipse)sender).Margin.Top);
                    break;
            }
        }
                
        
        void submenuEnter(double x, double y)
        {
            int threshold1 = 120;
            subsubmenuEllipse[0].Margin = new Thickness(x, y - threshold1, 0, 0);
            subsubmenuEllipse[1].Margin = new Thickness(x + threshold1, y, 0, 0);
            subsubmenuEllipse[2].Margin = new Thickness(x, y + threshold1, 0, 0);
            subsubmenuEllipse[3].Margin = new Thickness(x - threshold1, y, 0, 0);

            double x_ = (subsubmenuEllipse[0].Width - subsubmenuTextBlock[0].Width) / 2, y_ = (subsubmenuEllipse[0].Height - subsubmenuTextBlock[0].Height) / 2;
            subsubmenuTextBlock[0].Margin = new Thickness(x + x_, y + y_ - threshold1, 0, 0);
            subsubmenuTextBlock[1].Margin = new Thickness(x + x_ + threshold1, y + y_, 0, 0);
            subsubmenuTextBlock[2].Margin = new Thickness(x + x_, y + y_ + threshold1, 0, 0);
            subsubmenuTextBlock[3].Margin = new Thickness(x + x_ - threshold1, y + y_, 0, 0);

            for (int i = 0; i < 4; i++)
            {
                menuEllipse[i].Fill = lightlightgrayBrush;
                menuEllipse[i].Stroke = lightlightgrayBrush;
                menuTextBlock[i].Background = lightlightgrayBrush;
                menuTextBlock[i].Foreground = lightgrayBrush;
                submenuEllipse[i].Fill = lightgrayBrush;
                submenuEllipse[i].Stroke = lightgrayBrush;
                submenuTextBlock[i].Background = lightgrayBrush;
                submenuTextBlock[i].Foreground = grayBrush;
                subsubmenuEllipse[i].Fill = grayBrush;
                subsubmenuEllipse[i].Stroke = grayBrush;
                subsubmenuTextBlock[i].Foreground = blackBrush;
                subsubmenuTextBlock[i].Background = grayBrush;
                subsubmenuEllipse[i].Visibility = System.Windows.Visibility.Visible;
                subsubmenuTextBlock[i].Visibility = System.Windows.Visibility.Visible;
                if (depth == 2)
                    subsubmenuTextBlock[i].Text = menus.subsubmenus[0, currentMenu, i];
                else
                    subsubmenuTextBlock[i].Text = menus.subsubmenus[currentMenu, currentSubmenu, i];
            }
        }
        void submenuEnter()
        {
            submenuEllipse[currentSubmenu].Fill = blackBrush;
            submenuEllipse[currentSubmenu].Stroke = blackBrush;
            submenuTextBlock[currentSubmenu].Background = blackBrush;
            submenuTextBlock[currentSubmenu].Foreground = whiteBrush;
        }

        void submenuLeaveHandler(object sender, RoutedEventArgs e)
        {
            if (depth != 2)
                return;
            if (state != MARKING2)
                return;
            if (submenuTextBlock[currentSubmenu].IsMouseOver || submenuTextBlock[currentSubmenu].AreAnyTouchesCaptured)
                return;

            currentSubmenu = -1;
            submenuLeave();
        }

        void submenuLeave()
        {
            for (int i = 0; i < 4; i++)
            {
                submenuEllipse[i].Fill = grayBrush;
                submenuEllipse[i].Stroke = grayBrush;
                submenuTextBlock[i].Foreground = blackBrush;
                submenuTextBlock[i].Background = grayBrush;
            }
        }

        void submenuUpHandler(object sender, RoutedEventArgs e)
        {
            if (depth != 2)
                return;
            invocationUp();
            if (currentMenu * 4 + currentSubmenu == sessionTasks[currentTask] - 1)
            {
                //TODO log
                invocationUp();
                runNext();
            }
        }

        void submenuTextBlockEnterHandler(object sender, RoutedEventArgs e)
        {
            if (depth != 2)
                return;
            if (state != MARKING2)
                return;
            for (int i = 0; i < 4; i++)
            {
                if ((TextBlock)sender == submenuTextBlock[i])
                {
                    submenuEnterHandler(submenuEllipse[i], e);
                }
            }
        }

        void subsubmenuEnterHandler(object sender, RoutedEventArgs e){
            if (state != MARKING3)
                return;
            for (int i = 0; i < 4; i++)
            {
                if ((Ellipse)sender == subsubmenuEllipse[i])
                {
                    currentSubsubmenu = i;
                }
            }
            subsubmenuEnter();
        }
        void subsubmenuEnter()
        {
            subsubmenuEllipse[currentSubsubmenu].Fill = blackBrush;
            subsubmenuEllipse[currentSubsubmenu].Stroke = blackBrush;
            subsubmenuTextBlock[currentSubsubmenu].Background = blackBrush;
            subsubmenuTextBlock[currentSubsubmenu].Foreground = whiteBrush;
        }

        void subsubmenuLeaveHandler(object sender, RoutedEventArgs e)
        {
            if (state != MARKING3)
                return;
            if (subsubmenuTextBlock[currentSubsubmenu].IsMouseOver || subsubmenuTextBlock[currentSubsubmenu].AreAnyTouchesCaptured)
                return;
            
            currentSubsubmenu = -1;
            subsubmenuLeave();
        }

        void subsubmenuLeave()
        {
            for (int i = 0; i < 4; i++)
            {
                subsubmenuEllipse[i].Fill = grayBrush;
                subsubmenuEllipse[i].Stroke = grayBrush;
                subsubmenuTextBlock[i].Foreground = blackBrush;
                subsubmenuTextBlock[i].Background = grayBrush;
                if (depth == 2)
                    subsubmenuTextBlock[i].Text = menus.subsubmenus[0, currentMenu, i];
                else
                    subsubmenuTextBlock[i].Text = menus.subsubmenus[currentMenu, currentSubmenu, i];
            }
        }

        void subsubmenuUpHandler(object sender, RoutedEventArgs e)
        {
            invocationUp();
            //taskTextBlock.Text = "" + currentMenu + "," + currentSubmenu + "," + currentSubsubmenu + "==" + sessionTasks[currentTask]-1;
            if (currentMenu * 16 + currentSubmenu * 4 + currentSubsubmenu == sessionTasks[currentTask] - 1)
            {
                //TODO log
                invocationUp();
                runNext();
            }
        }


        void subsubmenuTextBlockEnterHandler(object sender, RoutedEventArgs e)
        {
            if (state != MARKING3)
                return;
            for (int i = 0; i < 4; i++)
            {
                if ((TextBlock)sender == subsubmenuTextBlock[i])
                {
                    subsubmenuEnterHandler(subsubmenuEllipse[i], e);
                }
            }
        }

        void fieldUpHandler(object sender, RoutedEventArgs e)
        {
            fieldUp();
        }
        void fieldUp()
        {
            if (depth == 2 && state != MARKING2)
                return;
            if (depth == 3 && state != MARKING3)
                return;

            state = DEFAULT;
            invocationUp();

            log += "" + markingState + " ";
            if (markingState[depth - 1] == sessionTasks[currentTask])
            {
                saveLog(log);
                log = "";
                runNext();
            }
        }
        
        void initializePanel()
        {
            Rectangle panelBackground = new Rectangle();
            panelBackground.BeginInit();
            panelBackground.Width = screenWidth - rectNx * rectWidth;
            panelBackground.Height = screenHeight;
            panelBackground.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            panelBackground.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelBackground.Margin = new Thickness(0, 0, 0, 0);
            panelBackground.Fill = grayBrush;
            panelBackground.EndInit();
            grid.Children.Add(panelBackground);

            participantTextBlock = new TextBlock();
            participantTextBlock.BeginInit();
            participantTextBlock.Width = 300;
            participantTextBlock.Height = 100;
            participantTextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            participantTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            participantTextBlock.Margin = new Thickness(50, 50, 0, 0);
            participantTextBlock.Background = grayBrush;            
            participantTextBlock.FontSize = 20;
            participantTextBlock.Foreground = whiteBrush;
            participantTextBlock.Text = "Participant #" + participantNumber + "\nSession " + tasks.curruntSession + "/" + tasks.maxSession;
            participantTextBlock.EndInit();
            grid.Children.Add(participantTextBlock);

            taskTextBlock = new TextBlock();
            taskTextBlock.BeginInit();
            taskTextBlock.Width = 300;
            taskTextBlock.Height = 100;
            taskTextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            taskTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            taskTextBlock.Margin = new Thickness(50, 200, 0, 0);
            taskTextBlock.Focusable = false;
            taskTextBlock.Background = whiteBrush;
            taskTextBlock.FontSize = 30;
            taskTextBlock.Foreground = blackBrush;
            taskTextBlock.Text = "START";
            taskTextBlock.EndInit();
            grid.Children.Add(taskTextBlock);

            state = NOTRUNNING;

            taskTextBlock.TouchDown += new EventHandler<TouchEventArgs>(start);
            taskTextBlock.MouseDown += new MouseButtonEventHandler(start);
        }

        void initializeTouchLine()
        {
            touchLine = new TouchLine(depth, grid) ;
        }

        /*
        void drawTouchLine(int index, double x1, double x2, double y1, double y2)
        {
            touchLine.setXY(0, x1, y1);
            touchLine.setXY(1, x2, y2);
        }*/

        void start(object sender, RoutedEventArgs e)
        {
            if (state != NOTRUNNING)
                return;

            sessionTasks = tasks.getOneSessionTasks();
            currentTask = -1;

            invocation.Visibility = System.Windows.Visibility.Visible;

            runNext();

            ///////
            touchLine.touchLine[0].X1 = screenWidth / 2;
            touchLine.touchLine[0].Y1 = 0;
            touchLine.touchLine[0].X2 = screenWidth / 2 + 100;
            touchLine.touchLine[0].Y2 = 0;
            touchLine.touchLine[0].Visibility = Visibility.Visible;
            //touchLine.drawLine(1);
        }

        void runNext()
        {
            state = DEFAULT;

            if (currentTask == sessionTasks.Length - 1)
                initializePanel();

            int problem = sessionTasks[++currentTask] - 1; // array index starts from 0
            participantTextBlock.Text = "Participant #" + participantNumber + "\nSession " + tasks.curruntSession + "/" + tasks.maxSession + "\nProblem " + (currentTask + 1) + "/" + (sessionTasks.Length + 1);

            if (depth == 2)
                taskTextBlock.Text = menus.subsubmenus[0, problem / 4, problem % 4];
            else
                taskTextBlock.Text = menus.subsubmenus[problem / 16, (problem / 4) % 4, problem % 4];
        }

        //TODO
        void saveLog(String log)
        {

        }
    }


    class TouchLine{
        public Line[] touchLine;
        public Ellipse[] touchEllepse;
        double[] Xs, Ys;
        Brush touchLineBrush = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
        int depth;

        public TouchLine(int depth, Grid grid){
            this.depth = depth;
            touchLine = new Line[depth];
            touchEllepse = new Ellipse[depth+1];
            Xs = new double[depth+1];
            Ys = new double[depth+1];

            for(int i = 0; i< depth; i++){
                touchLine[i] = new Line();
                touchLine[i].BeginInit();
                touchLine[i].StrokeThickness = 5;
                touchLine[i].Stroke = touchLineBrush;
                touchLine[i].VerticalAlignment = VerticalAlignment.Top;
                touchLine[i].HorizontalAlignment = HorizontalAlignment.Left;
                touchLine[i].Visibility = Visibility.Hidden;
                touchLine[i].EndInit();

                grid.Children.Add(touchLine[i]);
            }
            for (int i = 0; i < depth + 1; i++)
            {
                touchEllepse[i] = new Ellipse();
                touchEllepse[i].BeginInit();
                touchEllepse[i].StrokeThickness = 2;
                touchEllepse[i].Width = 20;
                touchEllepse[i].Height = 20;
                //touchEllepse[i].Fill = Color.FromArgb(255, 255, 255, 255);
                touchEllepse[i].Stroke = touchLineBrush;
                touchEllepse[i].VerticalAlignment = VerticalAlignment.Top;
                touchEllepse[i].HorizontalAlignment = HorizontalAlignment.Left;
                touchEllepse[i].Visibility = Visibility.Hidden;
                touchEllepse[i].EndInit();

                grid.Children.Add(touchEllepse[i]);
            }

        }

        public void setXY(int index, double x, double y){
            Xs[index] = x;
            Ys[index] = y;
        }

        public void drawLine(int level){
            for (int i = 0; i < level; i++) {
                double originalLenth = Math.Sqrt(Math.Pow(Ys[i + 1] - Ys[i], 2) + Math.Pow(Xs[i + 1] - Xs[i], 2));
                if (originalLenth == 0)
                    continue;
                touchLine[i].X1 = Xs[i] + 10 / originalLenth * (Xs[i + 1] - Xs[i]);
                touchLine[i].Y1 = Ys[i] + 10 / originalLenth * (Ys[i + 1] - Ys[i]);
                touchLine[i].X2 = Xs[i + 1] - 10 / originalLenth * (Xs[i + 1] - Xs[i]);
                touchLine[i].Y2 = Ys[i + 1] - 10 / originalLenth * (Ys[i + 1] - Ys[i]);
                touchLine[i].Visibility = Visibility.Visible;
            }

            for (int i = 0; i < level + 1; i++)
            {
                touchEllepse[i].Margin = new Thickness(Xs[i] - touchEllepse[i].Width / 2, Ys[i] - touchEllepse[i].Height / 2, 0, 0);
                touchEllepse[i].Visibility = Visibility.Visible;
            }
        }

        public void hideLine()
        {
            for (int i = 0; i < depth; i++)
                touchLine[i].Visibility = Visibility.Hidden;
            for (int i = 0; i < depth + 1; i++)
                touchEllepse[i].Visibility = Visibility.Hidden;
        }

    }
    
    class Menus
    {
        int depth;
        public Menus(int depth)
        {
            this.depth = depth;
        }

        public String[] menus = { "나라", "동물", "식물", "무생물" };
        public String[,] submenus =  {
                              {"아시아", "유럽",  "아메리카", "아프리카"},
                              {"육상동물",  "바다동물", "조류","곤충"},
                              {"꽃", "과일", "나무", "야채"},
                              {"전자제품","가구","의류","탈것"}
                              };
        public String[, ,] subsubmenus = { 
                                 {{"한국", "북한", "중국", "일본"}, {"영국", "프랑스", "독일", "스위스"}, {"캐나다", "미국", "멕시코", "브라질"}, {"가나", "가봉", "남아공", "에티오피아"}},
                                 {{"개", "고양이", "사자", "호랑이"},{"고등어", "갈치", "참치", "꽁치"}, { "기러기", "비둘기", "참새", "독수리"}, { "나비", "벌", "매미", "파리"}},
                                 {{"무궁화", "튤립", "코스모스", "벚꽃"},{ "사과", "배", "포도", "딸기", }, {"소나무", "은행나무", "단풍나무", "향나무"},{ "당근", "배추", "양파", "파"}},
                                 {{"냉장고", "전자레인지 ", "커피포트", "토스트기"}, {"옷장", "서랍장", "책장", "책상"}, {"티셔츠", "바지", "자켓", "양말"},{"자동차", "비행기", "기차", "자전거"}}
                                  };
    }

    class Tasks
    {
        int depth; // 2 or 3        
        int numMenuPerParticipant; // the number of menus for a participant, He or she will get tasks only for some menu, not entire menu.
        int numDuplcationInSession; // the number of same menu in a session. A session = duplication * menuForThisParticipant

        public int participantNumber; // key for which random array is selected
        public int maxSession; // the number of session for a participant.
        public int curruntSession;

        int[] menuForThisParticipant;

        public Tasks(int depth, int maxSession, int numMenuPerParticipant, int numDuplcationInSession, int participantNumber)
        {
            this.depth = depth;
            this.maxSession = maxSession;
            this.numMenuPerParticipant = numMenuPerParticipant;
            this.participantNumber = participantNumber;
            this.numDuplcationInSession = numDuplcationInSession;

            curruntSession = 0;
            setMenuForThisParticipants();
        }

        //depends on the participantNumber
        private void setMenuForThisParticipants()
        {
            int numMenu = (int)Math.Pow(4, depth);
            menuForThisParticipant = new int[numMenuPerParticipant];
            for (int i = 0; i < numMenuPerParticipant; i++)
            {
                if (depth == 2)
                    menuForThisParticipant[i] = randomPermutationDepth2[participantNumber, i];
                else
                    menuForThisParticipant[i] = randomPermutationDepth3[participantNumber, i];
            }
        }


        public int[] getOneSessionTasks()
        {
            curruntSession++;

            int[] oneSessionTasks = new int[numMenuPerParticipant];
            menuForThisParticipant.CopyTo(oneSessionTasks, 0);

            for (int i = 1; i < numDuplcationInSession; i++)
                oneSessionTasks = oneSessionTasks.Concat(menuForThisParticipant).ToArray();

            Random random = new Random();
            oneSessionTasks = oneSessionTasks.OrderBy(x => random.Next()).ToArray();

            return oneSessionTasks;
        }

        int[,] randomPermutationDepth2 = {
            {5,2,15,10,6,16,13,7,4,14,1,3,12,8,9,11},
            {15,5,13,16,11,12,4,8,14,10,9,2,6,1,7,3},
            {7,1,2,12,5,9,8,14,13,15,4,16,10,11,3,6},
            {6,5,11,13,2,9,7,14,10,4,3,15,12,1,16,8},
            {3,4,12,9,13,7,5,11,16,15,2,14,8,6,10,1},
            {11,10,15,5,8,12,3,9,16,2,7,6,13,1,4,14},
            {2,15,4,8,16,10,6,3,14,1,7,9,11,12,13,5},
            {10,6,14,8,2,16,15,7,4,11,12,3,13,1,9,5},
            {7,11,6,4,5,14,16,2,15,10,9,13,1,12,8,3},
            {7,1,15,3,4,2,5,12,14,6,11,10,16,13,8,9},
            {13,14,9,4,5,15,10,2,11,1,3,8,16,12,6,7},
            {13,9,7,5,10,3,16,4,11,2,8,1,6,15,12,14},
            {3,12,9,1,2,4,5,11,15,16,8,7,10,6,14,13},
            {9,15,7,2,8,10,5,3,12,14,11,4,1,6,16,13},
            {16,3,11,10,4,7,12,13,5,14,2,15,6,8,9,1}
            };
        int[,] randomPermutationDepth3 = {
            //{27,39,40,54,33,50,21,7,56,30,31,4,9,12,25,15,55,57,13,29,41,49,19,3,10,44,32,53,28,5,58,60,35,6,62,59,42,48,11,20,8,26,34,51,14,64,38,2,43,36,37,46,18,52,24,45,16,1,61,23,22,63,47,1},
            {19,15,27,7,42,55,31,28,40,34,52,9,54,2,36,8,38,41,4,24,32,12,61,17,63,21,46,57,58,22,20,26,30,45,43,16,56,33,50,47,48,59,62,10,5,14,3,49,44,6,53,29,35,39,23,60,13,25,64,11,18,37,51,1},            {59,48,23,35,21,22,9,33,45,32,63,20,50,2,47,7,62,8,60,44,53,26,40,55,24,61,58,51,25,12,39,43,18,36,37,5,16,14,4,46,3,28,13,11,15,41,56,52,34,57,17,54,42,49,27,29,64,1,38,6,30,10,31,19},            {48,39,64,31,32,33,38,23,17,36,24,9,55,62,29,12,8,3,5,25,1,14,53,37,35,56,20,52,30,41,58,44,61,22,50,63,13,34,43,60,26,7,16,4,28,10,54,6,45,27,15,59,40,18,49,46,21,2,57,42,11,47,19,51},            {10,18,28,21,53,57,47,48,11,24,36,49,26,50,64,27,23,25,45,41,15,40,30,20,44,35,13,22,37,33,63,34,31,16,42,52,9,7,19,51,1,17,29,43,2,32,55,3,56,6,62,14,38,61,58,12,39,4,8,54,59,46,5,60},            {57,10,3,55,16,34,53,60,31,63,43,21,2,50,37,17,32,47,59,38,51,54,26,6,49,5,61,19,7,36,14,33,1,27,56,15,62,28,8,41,35,23,12,46,48,9,18,40,24,45,44,29,30,11,13,52,58,20,39,42,4,22,64,25},            {19,5,20,43,31,3,44,10,63,45,9,36,14,51,52,37,49,59,27,17,2,4,54,1,33,16,64,30,39,6,23,13,38,29,57,22,34,8,53,55,21,48,28,25,50,24,42,60,58,35,18,56,7,11,32,26,46,41,15,62,40,12,47,61},            {55,43,48,7,21,54,64,17,23,60,34,24,52,62,53,63,15,36,9,6,12,19,37,42,26,18,49,5,1,10,41,22,56,45,29,32,46,14,39,20,59,11,3,27,47,61,13,33,57,44,2,30,51,58,31,40,8,50,16,25,4,38,35,28},            {51,24,13,63,50,1,30,32,18,2,21,11,3,12,6,58,7,22,52,37,42,17,5,4,53,41,35,45,31,23,28,26,44,48,49,43,57,38,10,16,46,40,29,19,54,64,25,36,9,15,61,47,62,33,60,59,39,34,8,14,56,55,27,20},            {26,64,15,54,38,3,18,63,11,30,6,5,24,2,59,37,25,35,46,20,57,62,44,22,23,43,34,53,1,50,9,19,13,21,33,61,56,51,42,60,28,7,40,14,10,41,17,36,39,48,32,16,47,45,4,49,55,8,31,27,29,52,12,58},            {45,23,38,55,17,43,40,14,56,52,8,16,54,2,53,12,35,33,10,32,19,59,4,13,28,21,60,1,34,31,48,39,7,30,57,29,51,22,15,20,63,46,18,3,6,26,47,25,41,36,27,5,49,50,24,11,44,58,62,9,61,37,64,42},            {30,34,55,15,54,50,48,25,44,59,49,52,61,24,38,43,4,39,5,26,58,2,60,20,18,35,8,23,22,41,51,62,14,7,12,56,11,13,9,42,64,3,40,6,29,36,16,27,37,10,1,46,33,57,28,45,31,21,53,63,32,47,19,17},            {41,50,3,1,18,10,58,60,45,53,44,56,25,37,29,16,51,42,31,62,11,2,22,33,5,52,8,34,4,64,28,26,61,48,15,30,47,20,49,43,35,46,14,54,19,57,39,23,32,63,7,21,24,59,38,55,9,13,12,17,40,36,27,6},            {15,6,41,32,39,47,16,29,27,21,63,25,8,42,19,23,22,58,50,40,62,24,33,26,3,12,31,20,55,4,56,64,1,60,51,36,57,46,10,13,61,18,9,11,37,53,43,44,34,59,35,49,14,54,2,45,28,38,48,30,5,7,17,52},            {6,20,35,64,5,54,57,33,48,61,11,8,1,24,28,46,36,42,21,56,47,40,23,17,52,7,19,29,14,53,22,34,63,37,32,4,62,18,41,59,3,49,2,12,39,26,27,45,30,60,50,15,13,58,55,51,31,38,25,9,16,43,10,44},            {44,18,39,60,58,40,46,22,38,47,41,25,2,4,43,42,24,13,55,26,27,7,63,30,31,36,1,56,61,21,20,14,57,8,32,54,45,28,53,29,51,12,11,64,48,33,19,9,15,3,17,35,23,6,62,37,10,49,16,59,5,34,50,52}        };


        /*
        public int[] getOneSessionTasks()
        {
            int[] oneSessionTasks = menuForThisParticipant.Concat(menuForThisParticipant).ToArray();

            Random random = new Random();
            oneSessionTasks = oneSessionTasks.OrderBy(x => random.Next()).ToArray();

            return oneSessionTasks;
        }*/
    }


}



/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastTap
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        String log = "";

        int depth = 3;  // 2: 4*4, 3: 4*4*4        
        int participantNumber = 0; // 0 ~ 15, key for which random series
        int maxSession = 10;
        int numMenuPerParticipant = 8;
        int numDuplcationInSession = 2;
        
        int state;
        const int NOTRUNNING = -1, DEFAULT = 0, INVOCATE = 1;

        int screenWidth = 1366, screenHeight = 768;                            
        int rectWidth, rectHeight, rectNx, rectNy, rectN;

        Menus menus;
        Tasks tasks;

        Rectangle[] rects;
        TextBlock[] texts;
        Rectangle invocation;
        Rectangle tempRectangle; //only for looking good
        TextBlock participantTextBlock;
        TextBlock taskTextBlock;
        int lastSelection = -1;
        int lastSelectionKey = 0;

        SolidColorBrush buttonDownBrush, invocationUpBrush, invocationDownBrush, grayBrush, whiteBrush, blackBrush;

        int[] sessionTasks;
        int currentTask;

        public MainWindow()
        {
            InitializeComponent();

            state = NOTRUNNING;
            menus = new Menus(depth);
            tasks = new Tasks(depth, maxSession, numMenuPerParticipant, numDuplcationInSession, participantNumber);

            initializeSize();            
            initializeBrushs();
            initializeRectsTexts();
            initializeInvocation();
            initializePanel();
            
        }

        private void initializeSize()
        {
            if(depth == 2){
                rectWidth = 170; rectHeight = 152; rectNx = 4; rectNy = 4; rectN = rectNx * rectNy;
            }else{
                rectWidth = 85; rectHeight = 76; rectNx = 8; rectNy = 8; rectN = rectNx * rectNy;
            }
        }

        private void initializeBrushs()
        {
            buttonDownBrush = new SolidColorBrush(Color.FromRgb(230, 190, 190));
            invocationUpBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            invocationDownBrush = new SolidColorBrush(Color.FromRgb(150, 150, 30));
            grayBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            whiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            blackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }
        
        private void initializeRectsTexts()
        {
            rects = new Rectangle[rectN];
            texts = new TextBlock[rectN];
            for (int i = 0; i < rectN; i++)
            {
                rects[i] = new Rectangle();
                rects[i].BeginInit();
                rects[i].Width = rectWidth;
                rects[i].Height = rectHeight;
                rects[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                rects[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                rects[i].Margin = new Thickness((screenWidth - rectNx * rectWidth) + (i % rectNx) * rectWidth, (i / rectNx) * rectHeight, 0, 0);
                rects[i].Visibility = System.Windows.Visibility.Visible;
                rects[i].EndInit();
                grid.Children.Add(rects[i]);

                rects[i].MouseDown += new MouseButtonEventHandler(buttonDownHandler);
                rects[i].MouseLeave += new MouseEventHandler(buttonUpHandler);
                rects[i].MouseUp += new MouseButtonEventHandler(buttonUpHandler);
                rects[i].MouseEnter += new MouseEventHandler(buttonDownHandler);
                
                rects[i].TouchDown += new EventHandler<TouchEventArgs>(buttonDownHandler);
                rects[i].TouchEnter += new EventHandler<TouchEventArgs>(buttonDownHandler);
                rects[i].TouchUp += new EventHandler<TouchEventArgs>(buttonUpHandler);
                rects[i].TouchLeave += new EventHandler<TouchEventArgs>(buttonUpHandler);                

                texts[i] = new TextBlock();
                texts[i].BeginInit();
                if (depth == 2)
                    texts[i].Text = menus.subsubmenus[0, i / 4, i % 4];
                else
                    texts[i].Text = menus.subsubmenus[i / 16, (i / 4) % 4, i % 4];
                texts[i].Width = rectWidth;
                texts[i].Height = rectHeight;
                texts[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                texts[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;                
                texts[i].TextAlignment = TextAlignment.Center;
                texts[i].FontSize = 12;
                texts[i].Padding = new Thickness(0, (rectHeight - texts[i].FontSize) / 2, 0, (rectHeight - texts[i].FontSize) / 2);
                texts[i].Margin = new Thickness((screenWidth - rectNx * rectWidth) + (i % rectNx) * rectWidth, (i / rectNx) * rectHeight, 0, 0);
                //texts[i].Focusable = false;
                texts[i].Visibility = System.Windows.Visibility.Hidden;
                texts[i].EndInit();
                grid.Children.Add(texts[i]);

                texts[i].MouseDown += new MouseButtonEventHandler(buttonDownHandler);
                texts[i].MouseLeave += new MouseEventHandler(buttonUpHandler);
                texts[i].MouseUp += new MouseButtonEventHandler(buttonUpHandler);
                texts[i].MouseEnter += new MouseEventHandler(buttonDownHandler);

                texts[i].TouchDown += new EventHandler<TouchEventArgs>(buttonDownHandler);
                texts[i].TouchEnter += new EventHandler<TouchEventArgs>(buttonDownHandler);
                texts[i].TouchUp += new EventHandler<TouchEventArgs>(buttonUpHandler);
                texts[i].TouchLeave += new EventHandler<TouchEventArgs>(buttonUpHandler);                

            }
        }
        
        private void initializeInvocation()
        {   
            invocation = new Rectangle();
            invocation.BeginInit();
            invocation.Width = 170; //rectWidth;
            invocation.Height = screenHeight - rectNy * rectHeight;
            invocation.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            invocation.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            invocation.Margin = new Thickness((screenWidth - rectNx * rectWidth), rectNy * rectHeight, 0, 0);
            invocation.Stroke = grayBrush;
            invocation.Fill = invocationUpBrush;
            invocation.EndInit();
            grid.Children.Add(invocation);

            invocation.MouseLeftButtonDown += new MouseButtonEventHandler(invocateHandler);
            //invocation.MouseLeave += new MouseEventHandler(cancelInvocationHandler);
            //invocation.MouseLeftButtonUp += new MouseButtonEventHandler(cancelInvocationHandler);
            invocation.MouseRightButtonDown += new MouseButtonEventHandler(cancelInvocationHandler);

            invocation.TouchDown += new EventHandler<TouchEventArgs>(invocateHandler);
            invocation.TouchEnter += new EventHandler<TouchEventArgs>(invocateHandler);
            invocation.TouchUp += new EventHandler<TouchEventArgs>(cancelInvocationHandler);
            invocation.TouchLeave += new EventHandler<TouchEventArgs>(cancelInvocationHandler);


            tempRectangle = new Rectangle(); //just for looking good
            tempRectangle.BeginInit();
            tempRectangle.Width = screenWidth - invocation.Width;
            tempRectangle.Height = invocation.Height;
            tempRectangle.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            tempRectangle.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            tempRectangle.Margin = new Thickness((screenWidth - rectNx * rectWidth) + invocation.Width, rectNy * rectHeight, 0, 0);
            tempRectangle.Stroke = grayBrush;
            tempRectangle.Visibility = System.Windows.Visibility.Hidden;
            tempRectangle.EndInit();
            grid.Children.Add(tempRectangle);
        }

        // Handler for invocation, by both mouse and touch event
        void invocateHandler(object sender, RoutedEventArgs e)
        {
            lastSelection = -1;
            if (state != DEFAULT)
                return;

            state = INVOCATE;
            invocation.Fill = invocationUpBrush;
            for (int i = 0; i < rectN; i++)
            {
                rects[i].Stroke = grayBrush;
                texts[i].Visibility = System.Windows.Visibility.Visible;
                tempRectangle.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // Handler for cancel invocation, by both mouse and touch event        
        void cancelInvocationHandler(object sender, RoutedEventArgs e)
        {
            cacelInvocation();
        }
        void cacelInvocation()
        {
            if (state == DEFAULT)
                return;

            state = DEFAULT;
            invocation.Fill = invocationUpBrush;
            for (int i = 0; i < rectN; i++)
            {
                if (i != lastSelection)
                {
                    rects[i].Stroke = whiteBrush;
                    rects[i].Fill = whiteBrush;
                    texts[i].Visibility = System.Windows.Visibility.Hidden;
                    tempRectangle.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            invisibleLastSeclection(lastSelection, ++lastSelectionKey);
        }

        private async void invisibleLastSeclection(int lastSelection, int lastSelectionKey)
        {
            await Task.Delay(500);
            if (state == INVOCATE)
                return;
            if (lastSelectionKey != this.lastSelectionKey)
                return;
            if (lastSelectionKey > 10)
                lastSelectionKey = 0;
            if (lastSelection == -1)
                return;
            rects[lastSelection].Stroke = whiteBrush;
            rects[lastSelection].Fill = whiteBrush;
            texts[lastSelection].Visibility = System.Windows.Visibility.Hidden;
        }


        // Handler for excute, of each FastTap button, by both mouse and touch event
        // When state is INVOKE, excute immediately. When state is DEFAULT, do nothing
        void buttonDownHandler(object sender, RoutedEventArgs e)
        {
            if (state != INVOCATE)
                return;

            for (int i = 0; i < rectN; i++)
            {
                if (sender == texts[i])
                {
                    lastSelection = i;
                    rects[i].Fill = buttonDownBrush;
                    log += "" + i + " ";
                    if (i == sessionTasks[currentTask]-1) //because of array index
                    {
                        participantTextBlock.Text = "CORRECT!";
                        saveLog(log);
                        log = "";
                        runNext();
                    }
                }
            }
        }

        // Handler for button down, change the button color and
        void buttonUpHandler(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < rectN; i++)
                if (sender == texts[i])
                    rects[i].Fill = whiteBrush;
        }

        void initializePanel()
        {
            Rectangle panelBackground = new Rectangle();
            panelBackground.BeginInit();
            panelBackground.Width = screenWidth - rectNx * rectWidth;
            panelBackground.Height = screenHeight;
            panelBackground.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            panelBackground.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelBackground.Margin = new Thickness(0, 0, 0, 0);
            panelBackground.Fill = grayBrush;
            panelBackground.EndInit();
            grid.Children.Add(panelBackground);

            participantTextBlock = new TextBlock();
            participantTextBlock.BeginInit();
            participantTextBlock.Width = 300;
            participantTextBlock.Height = 100;
            participantTextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            participantTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;            
            participantTextBlock.FontSize = 20;            
            participantTextBlock.Margin = new Thickness(50, 50, 0, 0);
            participantTextBlock.Focusable = false;
            participantTextBlock.Background = grayBrush;            
            participantTextBlock.Foreground = whiteBrush;
            participantTextBlock.Text = "Participant #" + participantNumber + "\nSession " + tasks.curruntSession + "/" + tasks.maxSession + sessionTasks;
            participantTextBlock.EndInit();
            grid.Children.Add(participantTextBlock);

            taskTextBlock = new TextBlock();
            taskTextBlock.BeginInit();
            taskTextBlock.Width = 300;
            taskTextBlock.Height = 100;
            taskTextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            taskTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            taskTextBlock.FontSize = 30;
            taskTextBlock.TextAlignment = TextAlignment.Center;
            taskTextBlock.Padding = new Thickness(0, (taskTextBlock.Height - taskTextBlock.FontSize) / 2, 0, (taskTextBlock.Height - taskTextBlock.FontSize) / 2);
            taskTextBlock.Margin = new Thickness(50, 200, 0, 0);
            taskTextBlock.Focusable = false;
            taskTextBlock.Background = whiteBrush;            
            taskTextBlock.Foreground = blackBrush;
            taskTextBlock.Text = "START";
            taskTextBlock.EndInit();            
            grid.Children.Add(taskTextBlock);

            state = NOTRUNNING;

            taskTextBlock.TouchDown += new EventHandler<TouchEventArgs>(startHandler);
            taskTextBlock.MouseDown += new MouseButtonEventHandler(startHandler);
        }

        void startHandler(object sender, RoutedEventArgs e)
        {
            start();
        }

        void start()
        {
            if (state != NOTRUNNING)
                return;

            sessionTasks = tasks.getOneSessionTasks();
            currentTask = -1;
            runNext();
        }

        void runNext()
        {
            cacelInvocation();
            lastSelection = -1;

            if (currentTask == sessionTasks.Length - 1)
            {
                state = NOTRUNNING;
                start();
            }
                        
            int problem = sessionTasks[++currentTask] - 1; // array index starts from 0
            participantTextBlock.Text = "Participant #" + participantNumber + "\nSession " + tasks.curruntSession + "/" + tasks.maxSession + "\nProblem " + (currentTask+1)+"/"+(sessionTasks.Length+1);

            if (depth == 2)
                taskTextBlock.Text = menus.subsubmenus[0, problem / 4, problem % 4];
            else
                taskTextBlock.Text = menus.subsubmenus[problem / 16, (problem / 4) % 4, problem % 4];
        }

        //TODO
        void saveLog(String log)
        {

        }
    }

    class Menus
    {
        int depth;
        public Menus(int depth)
        {
            this.depth = depth;
        }

        public String[] menus = {"나라", "동물", "식물", "무생물"};
        public String[,] submenus =  {
                              {"아시아", "유럽",  "아메리카", "아프리카"},
                              {"육상동물",  "바다동물", "조류","곤충"},
                              {"꽃", "과일", "나무", "야채"},
                              {"전자제품","가구","의류","탈것"}
                              };
        public String[,,] subsubmenus = { 
                                 {{"한국", "북한", "중국", "일본"}, {"영국", "프랑스", "독일", "스위스"}, {"캐나다", "미국", "멕시코", "브라질"}, {"가나", "가봉", "남아공", "에티오피아"}},
                                 {{"개", "고양이", "사자", "호랑이"},{"고등어", "갈치", "참치", "꽁치"}, { "기러기", "비둘기", "참새", "독수리"}, { "나비", "벌", "매미", "파리"}},
                                 {{"무궁화", "튤립", "코스모스", "벚꽃"},{ "사과", "배", "포도", "딸기", }, {"소나무", "은행나무", "단풍나무", "향나무"},{ "당근", "배추", "양파", "파"}},
                                 {{"냉장고", "전자레인지 ", "커피포트", "토스트기"}, {"옷장", "서랍장", "책장", "책상"}, {"티셔츠", "바지", "자켓", "양말"},{"자동차", "비행기", "기차", "자전거"}}
                                  };
    }

    class Tasks
    {
        int depth; // 2 or 3        
        int numMenuPerParticipant; // the number of menus for a participant, He or she will get tasks only for some menu, not entire menu.
        int numDuplcationInSession; // the number of same menu in a session. A session = duplication * menuForThisParticipant
        
        public int participantNumber; // key for which random array is selected
        public int maxSession; // the number of session for a participant.
        public int curruntSession;

        int[] menuForThisParticipant;

        public Tasks(int depth, int maxSession, int numMenuPerParticipant, int numDuplcationInSession, int participantNumber)
        {
            this.depth = depth;
            this.maxSession = maxSession;
            this.numMenuPerParticipant = numMenuPerParticipant;
            this.participantNumber = participantNumber;
            this.numDuplcationInSession = numDuplcationInSession;

            curruntSession = 0;
            setMenuForThisParticipants();
        }

        //depends on the participantNumber
        private void setMenuForThisParticipants()
        {
            int numMenu = (int)Math.Pow(4, depth);
            menuForThisParticipant = new int[numMenuPerParticipant];
            for (int i = 0; i < numMenuPerParticipant; i++)
            {
                if(depth == 2)
                    menuForThisParticipant[i] = randomPermutationDepth2[participantNumber, i];                
                else
                    menuForThisParticipant[i] = randomPermutationDepth3[participantNumber, i];                
            }            
        }
        

        public int[] getOneSessionTasks()
        {
            curruntSession++;

            int[] oneSessionTasks = new int[numMenuPerParticipant];
            menuForThisParticipant.CopyTo(oneSessionTasks, 0);

            for (int i = 0; i < numDuplcationInSession; i++)
                oneSessionTasks = oneSessionTasks.Concat(menuForThisParticipant).ToArray();

            Random random = new Random();
            oneSessionTasks = oneSessionTasks.OrderBy(x => random.Next()).ToArray();

            return oneSessionTasks;
        }

        int[,] randomPermutationDepth2 = {
            {5,2,15,10,6,16,13,7,4,14,1,3,12,8,9,11},
            {15,5,13,16,11,12,4,8,14,10,9,2,6,1,7,3},
            {7,1,2,12,5,9,8,14,13,15,4,16,10,11,3,6},
            {6,5,11,13,2,9,7,14,10,4,3,15,12,1,16,8},
            {3,4,12,9,13,7,5,11,16,15,2,14,8,6,10,1},
            {11,10,15,5,8,12,3,9,16,2,7,6,13,1,4,14},
            {2,15,4,8,16,10,6,3,14,1,7,9,11,12,13,5},
            {10,6,14,8,2,16,15,7,4,11,12,3,13,1,9,5},
            {7,11,6,4,5,14,16,2,15,10,9,13,1,12,8,3},
            {7,1,15,3,4,2,5,12,14,6,11,10,16,13,8,9},
            {13,14,9,4,5,15,10,2,11,1,3,8,16,12,6,7},
            {13,9,7,5,10,3,16,4,11,2,8,1,6,15,12,14},
            {3,12,9,1,2,4,5,11,15,16,8,7,10,6,14,13},
            {9,15,7,2,8,10,5,3,12,14,11,4,1,6,16,13},
            {16,3,11,10,4,7,12,13,5,14,2,15,6,8,9,1}
            };
        int[,] randomPermutationDepth3 = {
            //{27,39,40,54,33,50,21,7,56,30,31,4,9,12,25,15,55,57,13,29,41,49,19,3,10,44,32,53,28,5,58,60,35,6,62,59,42,48,11,20,8,26,34,51,14,64,38,2,43,36,37,46,18,52,24,45,16,1,61,23,22,63,47,1},
            {19,15,27,7,42,55,31,28,40,34,52,9,54,2,36,8,38,41,4,24,32,12,61,17,63,21,46,57,58,22,20,26,30,45,43,16,56,33,50,47,48,59,62,10,5,14,3,49,44,6,53,29,35,39,23,60,13,25,64,11,18,37,51,1},
            {59,48,23,35,21,22,9,33,45,32,63,20,50,2,47,7,62,8,60,44,53,26,40,55,24,61,58,51,25,12,39,43,18,36,37,5,16,14,4,46,3,28,13,11,15,41,56,52,34,57,17,54,42,49,27,29,64,1,38,6,30,10,31,19},
            {48,39,64,31,32,33,38,23,17,36,24,9,55,62,29,12,8,3,5,25,1,14,53,37,35,56,20,52,30,41,58,44,61,22,50,63,13,34,43,60,26,7,16,4,28,10,54,6,45,27,15,59,40,18,49,46,21,2,57,42,11,47,19,51},
            {10,18,28,21,53,57,47,48,11,24,36,49,26,50,64,27,23,25,45,41,15,40,30,20,44,35,13,22,37,33,63,34,31,16,42,52,9,7,19,51,1,17,29,43,2,32,55,3,56,6,62,14,38,61,58,12,39,4,8,54,59,46,5,60},
            {57,10,3,55,16,34,53,60,31,63,43,21,2,50,37,17,32,47,59,38,51,54,26,6,49,5,61,19,7,36,14,33,1,27,56,15,62,28,8,41,35,23,12,46,48,9,18,40,24,45,44,29,30,11,13,52,58,20,39,42,4,22,64,25},
            {19,5,20,43,31,3,44,10,63,45,9,36,14,51,52,37,49,59,27,17,2,4,54,1,33,16,64,30,39,6,23,13,38,29,57,22,34,8,53,55,21,48,28,25,50,24,42,60,58,35,18,56,7,11,32,26,46,41,15,62,40,12,47,61},
            {55,43,48,7,21,54,64,17,23,60,34,24,52,62,53,63,15,36,9,6,12,19,37,42,26,18,49,5,1,10,41,22,56,45,29,32,46,14,39,20,59,11,3,27,47,61,13,33,57,44,2,30,51,58,31,40,8,50,16,25,4,38,35,28},
            {51,24,13,63,50,1,30,32,18,2,21,11,3,12,6,58,7,22,52,37,42,17,5,4,53,41,35,45,31,23,28,26,44,48,49,43,57,38,10,16,46,40,29,19,54,64,25,36,9,15,61,47,62,33,60,59,39,34,8,14,56,55,27,20},
            {26,64,15,54,38,3,18,63,11,30,6,5,24,2,59,37,25,35,46,20,57,62,44,22,23,43,34,53,1,50,9,19,13,21,33,61,56,51,42,60,28,7,40,14,10,41,17,36,39,48,32,16,47,45,4,49,55,8,31,27,29,52,12,58},
            {45,23,38,55,17,43,40,14,56,52,8,16,54,2,53,12,35,33,10,32,19,59,4,13,28,21,60,1,34,31,48,39,7,30,57,29,51,22,15,20,63,46,18,3,6,26,47,25,41,36,27,5,49,50,24,11,44,58,62,9,61,37,64,42},
            {30,34,55,15,54,50,48,25,44,59,49,52,61,24,38,43,4,39,5,26,58,2,60,20,18,35,8,23,22,41,51,62,14,7,12,56,11,13,9,42,64,3,40,6,29,36,16,27,37,10,1,46,33,57,28,45,31,21,53,63,32,47,19,17},
            {41,50,3,1,18,10,58,60,45,53,44,56,25,37,29,16,51,42,31,62,11,2,22,33,5,52,8,34,4,64,28,26,61,48,15,30,47,20,49,43,35,46,14,54,19,57,39,23,32,63,7,21,24,59,38,55,9,13,12,17,40,36,27,6},
            {15,6,41,32,39,47,16,29,27,21,63,25,8,42,19,23,22,58,50,40,62,24,33,26,3,12,31,20,55,4,56,64,1,60,51,36,57,46,10,13,61,18,9,11,37,53,43,44,34,59,35,49,14,54,2,45,28,38,48,30,5,7,17,52},
            {6,20,35,64,5,54,57,33,48,61,11,8,1,24,28,46,36,42,21,56,47,40,23,17,52,7,19,29,14,53,22,34,63,37,32,4,62,18,41,59,3,49,2,12,39,26,27,45,30,60,50,15,13,58,55,51,31,38,25,9,16,43,10,44},
            {44,18,39,60,58,40,46,22,38,47,41,25,2,4,43,42,24,13,55,26,27,7,63,30,31,36,1,56,61,21,20,14,57,8,32,54,45,28,53,29,51,12,11,64,48,33,19,9,15,3,17,35,23,6,62,37,10,49,16,59,5,34,50,52}
        };


        
    }

}

*/