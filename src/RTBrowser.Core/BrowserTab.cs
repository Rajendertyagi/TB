<UserControl
    x:Class="RTBrowser.UI.Controls.NavigationBar"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Height="30">

    <UserControl.Resources>

        <Style TargetType="Button">

            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>

            <Style.Triggers>

                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Opacity" Value="0.78"/>
                </Trigger>

                <Trigger Property="IsPressed" Value="True">
                    <Setter Property="Opacity" Value="0.58"/>
                </Trigger>

            </Style.Triggers>

        </Style>

    </UserControl.Resources>

    <Border
        Background="#101113"
        BorderBrush="#1A1B1E"
        BorderThickness="0,0,0,1">

        <Grid Margin="8,2,8,2">

            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- LEFT -->

            <StackPanel
                Grid.Column="0"
                Orientation="Horizontal"
                VerticalAlignment="Center">

                <!-- BACK -->

                <Button
                    x:Name="BackButton"
                    Width="20"
                    Height="20"
                    Margin="0,0,3,0">

                    <TextBlock
                        Text="←"
                        FontSize="10"
                        Foreground="#AEB4BC"
                        HorizontalAlignment="Center"
                        VerticalAlignment="Center"/>

                </Button>

                <!-- FORWARD -->

                <Button
                    x:Name="ForwardButton"
                    Width="20"
                    Height="20"
                    Margin="0,0,3,0">

                    <TextBlock
                        Text="→"
                        FontSize="10"
                        Foreground="#AEB4BC"
                        HorizontalAlignment="Center"
                        VerticalAlignment="Center"/>

                </Button>

                <!-- REFRESH -->

                <Button
                    x:Name="RefreshButton"
                    Width="20"
                    Height="20"
                    Margin="0,0,6,0">

                    <TextBlock
                        Text="↻"
                        FontSize="10"
                        Foreground="#AEB4BC"
                        HorizontalAlignment="Center"
                        VerticalAlignment="Center"/>

                </Button>

            </StackPanel>

            <!-- OMNIBOX -->

            <Border
                x:Name="OmniboxBorder"
                Grid.Column="1"
                Height="24"
                Background="#17181B"
                BorderBrush="#26282D"
                BorderThickness="1"
                CornerRadius="6"
                VerticalAlignment="Center">

                <Grid>

                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="26"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>

                    <!-- SECURITY -->

                    <TextBlock
                        Grid.Column="0"
                        Text="◦"
                        FontSize="12"
                        Foreground="#5EA1FF"
                        HorizontalAlignment="Center"
                        VerticalAlignment="Center"/>

                    <!-- ADDRESS -->

                    <TextBox
                        x:Name="AddressBar"
                        Grid.Column="1"
                        Margin="0,0,8,0"
                        Background="Transparent"
                        BorderThickness="0"
                        CaretBrush="White"
                        FontSize="11"
                        Foreground="#E6E6E6"
                        VerticalContentAlignment="Center"
                        Padding="0"/>

                </Grid>

            </Border>

            <!-- RIGHT -->

            <Border
                Grid.Column="2"
                Width="8"
                Height="8"
                Margin="8,0,0,0"
                Background="#4C8DFF"
                CornerRadius="4"
                VerticalAlignment="Center"/>

        </Grid>

    </Border>

</UserControl>
