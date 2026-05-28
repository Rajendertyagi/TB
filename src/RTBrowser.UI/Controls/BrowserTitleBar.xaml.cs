<UserControl
    x:Class="RTBrowser.UI.Controls.BrowserTitleBar"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Height="{StaticResource TitleBarHeight}">

    <Border
        Background="{StaticResource BackgroundPrimaryBrush}">

        <Grid Height="{StaticResource TitleBarHeight}">

            <Grid.ColumnDefinitions>

                <ColumnDefinition Width="*" />

                <ColumnDefinition Width="Auto" />

            </Grid.ColumnDefinitions>

            <!-- LEFT -->

            <StackPanel
                Grid.Column="0"
                Orientation="Horizontal"
                VerticalAlignment="Center"
                Margin="8,0">

                <Button
                    Width="28"
                    Height="28"
                    Margin="0,0,4,0"
                    Background="Transparent"
                    BorderThickness="0">

                    <TextBlock
                        Text="⌂"
                        FontSize="12"
                        Foreground="{StaticResource TextSecondaryBrush}"
                        VerticalAlignment="Center"
                        HorizontalAlignment="Center" />

                </Button>

                <Button
                    Width="28"
                    Height="28"
                    Margin="0,0,4,0"
                    Background="Transparent"
                    BorderThickness="0">

                    <TextBlock
                        Text="←"
                        FontSize="12"
                        Foreground="{StaticResource TextSecondaryBrush}"
                        VerticalAlignment="Center"
                        HorizontalAlignment="Center" />

                </Button>

                <Button
                    Width="28"
                    Height="28"
                    Margin="0,0,4,0"
                    Background="Transparent"
                    BorderThickness="0">

                    <TextBlock
                        Text="→"
                        FontSize="12"
                        Foreground="{StaticResource TextSecondaryBrush}"
                        VerticalAlignment="Center"
                        HorizontalAlignment="Center" />

                </Button>

                <Button
                    Width="28"
                    Height="28"
                    Background="Transparent"
                    BorderThickness="0">

                    <TextBlock
                        Text="↻"
                        FontSize="12"
                        Foreground="{StaticResource TextSecondaryBrush}"
                        VerticalAlignment="Center"
                        HorizontalAlignment="Center" />

                </Button>

            </StackPanel>

            <!-- RIGHT -->

            <StackPanel
                Grid.Column="1"
                Orientation="Horizontal"
                VerticalAlignment="Center"
                Margin="0,0,8,0">

                <Button
                    Width="28"
                    Height="28"
                    Margin="0,0,4,0"
                    Background="Transparent"
                    BorderThickness="0">

                    <TextBlock
                        Text="◻"
                        FontSize="11"
                        Foreground="{StaticResource TextSecondaryBrush}"
                        VerticalAlignment="Center"
                        HorizontalAlignment="Center" />

                </Button>

                <Button
                    Width="28"
                    Height="28"
                    Background="Transparent"
                    BorderThickness="0">

                    <TextBlock
                        Text="✕"
                        FontSize="11"
                        Foreground="{StaticResource TextSecondaryBrush}"
                        VerticalAlignment="Center"
                        HorizontalAlignment="Center" />

                </Button>

            </StackPanel>

        </Grid>

    </Border>

</UserControl>
