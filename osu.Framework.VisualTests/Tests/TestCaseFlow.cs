﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Framework.Threading;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.VisualTests.Tests
{
    internal class TestCaseFlow : TestCase
    {
        public override string Description => "Test lots of different settings for Flow Containers";

        private FlowTestCase current;
        private FillDirectionDropdown selectionDropdown;

        private Anchor childAnchor = Anchor.TopLeft;
        private AnchorDropdown anchorDropdown;

        private Anchor childOrigin = Anchor.TopLeft;
        private AnchorDropdown originDropdown;

        private FillFlowContainer fillContainer;
        private ScheduledDelegate scheduledAdder;
        private bool addChildren;

        public override void Reset()
        {
            base.Reset();

            scheduledAdder?.Cancel();

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Width = 0.2f,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Depth = float.MinValue,
                Children = new[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SpriteText { Text = @"Fill mode" },
                            selectionDropdown = new FillDirectionDropdown
                            {
                                RelativeSizeAxes = Axes.X,
                                Items = Enum.GetValues(typeof(FlowTestCase)).Cast<FlowTestCase>()
                                            .Select(value => new KeyValuePair<string, FlowTestCase>(value.ToString(), value)),
                            },
                            new SpriteText { Text = @"Child anchor" },
                            anchorDropdown = new AnchorDropdown
                            {
                                RelativeSizeAxes = Axes.X,
                                Items = new[]
                                {
                                    Anchor.TopLeft,
                                    Anchor.TopCentre,
                                    Anchor.TopRight,
                                    Anchor.CentreLeft,
                                    Anchor.Centre,
                                    Anchor.CentreRight,
                                    Anchor.BottomLeft,
                                    Anchor.BottomCentre,
                                    Anchor.BottomRight,
                                }.Select(anchor => new KeyValuePair<string, Anchor>(anchor.ToString(), anchor)),
                            },
                            new SpriteText { Text = @"Child origin" },
                            originDropdown = new AnchorDropdown
                            {
                                RelativeSizeAxes = Axes.X,
                                Items = new[]
                                {
                                    Anchor.TopLeft,
                                    Anchor.TopCentre,
                                    Anchor.TopRight,
                                    Anchor.CentreLeft,
                                    Anchor.Centre,
                                    Anchor.CentreRight,
                                    Anchor.BottomLeft,
                                    Anchor.BottomCentre,
                                    Anchor.BottomRight,
                                }.Select(anchor => new KeyValuePair<string, Anchor>(anchor.ToString(), anchor)),
                            },
                        }
                    }
                }
            });

            selectionDropdown.SelectedValue.ValueChanged += newValue =>
            {
                current = newValue;
                Reset();
            };

            changeTest(current);
        }

        protected override void Update()
        {
            base.Update();

            if (childAnchor != anchorDropdown.SelectedValue)
            {
                childAnchor = anchorDropdown.SelectedValue;
                foreach (var child in fillContainer.Children)
                    child.Anchor = childAnchor;
            }

            if (childOrigin != originDropdown.SelectedValue)
            {
                childOrigin = originDropdown.SelectedValue;
                foreach (var child in fillContainer.Children)
                    child.Origin = childOrigin;
            }
        }

        private void changeTest(FlowTestCase testCase)
        {
            current = testCase;
            var method =
                GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).SingleOrDefault(m => m.GetCustomAttribute<FlowTestCaseAttribute>()?.TestCase == testCase);
            if (method != null)
                method.Invoke(this, new object[0]);
        }

        private void buildTest(FillDirection dir, Vector2 spacing)
        {
            Add(new Container
            {
                Padding = new MarginPadding(25f),
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    fillContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        AutoSizeAxes = Axes.None,
                        Direction = dir,
                        Spacing = spacing,
                    }
                }
            });

            AddToggleStep("Rotate Container", state => { fillContainer.RotateTo(state ? 45f : 0, 1000); });
            AddToggleStep("Scale Container", state => { fillContainer.ScaleTo(state ? 1.2f : 1f, 1000); });
            AddToggleStep("Shear Container", state => { fillContainer.Shear = state ? new Vector2(0.5f, 0f) : new Vector2(0f, 0f); });
            AddToggleStep("Center Container Anchor", state => { fillContainer.Anchor = state ? Anchor.Centre : Anchor.TopLeft; });
            AddToggleStep("Center Container Origin", state => { fillContainer.Origin = state ? Anchor.Centre : Anchor.TopLeft; });
            AddToggleStep("Autosize Container", state =>
            {
                if (state)
                {
                    fillContainer.RelativeSizeAxes = Axes.None;
                    fillContainer.AutoSizeAxes = Axes.Both;
                }
                else
                {
                    fillContainer.AutoSizeAxes = Axes.None;
                    fillContainer.RelativeSizeAxes = Axes.Both;
                    fillContainer.Width = 1;
                    fillContainer.Height = 1;
                }
            });
            AddToggleStep("Rotate children", state =>
            {
                if (state)
                {
                    foreach (var child in fillContainer.Children)
                        child.RotateTo(45f, 1000);
                }
                else
                {
                    foreach (var child in fillContainer.Children)
                        child.RotateTo(0f, 1000);
                }
            });
            AddToggleStep("Shear children", state =>
            {
                if (state)
                {
                    foreach (var child in fillContainer.Children)
                        child.Shear = new Vector2(0.2f, 0.2f);
                }
                else
                {
                    foreach (var child in fillContainer.Children)
                        child.Shear = Vector2.Zero;
                }
            });
            AddToggleStep("Scale children", state =>
            {
                if (state)
                {
                    foreach (var child in fillContainer.Children)
                        child.ScaleTo(1.25f, 1000);
                }
                else
                {
                    foreach (var child in fillContainer.Children)
                        child.ScaleTo(1f, 1000);
                }
            });

            Add(new Box { Colour = Color4.HotPink, Width = 3, Height = 3, Position = Content.ToSpaceOfOtherDrawable(fillContainer.BoundingBox.TopLeft, this), Origin = Anchor.Centre });
            Add(new Box { Colour = Color4.HotPink, Width = 3, Height = 3, Position = Content.ToSpaceOfOtherDrawable(fillContainer.BoundingBox.TopRight, this), Origin = Anchor.Centre });
            Add(new Box { Colour = Color4.HotPink, Width = 3, Height = 3, Position = Content.ToSpaceOfOtherDrawable(fillContainer.BoundingBox.BottomLeft, this), Origin = Anchor.Centre });
            Add(new Box { Colour = Color4.HotPink, Width = 3, Height = 3, Position = Content.ToSpaceOfOtherDrawable(fillContainer.BoundingBox.BottomRight, this), Origin = Anchor.Centre });

            AddToggleStep("Stop adding children", state => { addChildren = state; });

            scheduledAdder?.Cancel();
            scheduledAdder = Scheduler.AddDelayed(
                () =>
                {
                    if (fillContainer.Parent == null)
                        scheduledAdder.Cancel();

                    if (addChildren)
                    {
                        fillContainer.Invalidate();
                    }

                    if (fillContainer.Children.Count() < 1000 && !addChildren)
                    {
                        fillContainer.Add(new Container
                        {
                            Anchor = childAnchor,
                            Origin = childOrigin,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Width = 50,
                                    Height = 50,
                                    Colour = Color4.White
                                },
                                new SpriteText
                                {
                                    Colour = Color4.Black,
                                    RelativePositionAxes = Axes.Both,
                                    Position = new Vector2(0.5f, 0.5f),
                                    Origin = Anchor.Centre,
                                    Text = fillContainer.Children.Count().ToString()
                                }
                            }
                        });
                    }
                },
                100,
                true
            );
        }

        [FlowTestCase(FlowTestCase.Full)]
        private void test1()
        {
            buildTest(FillDirection.Full, new Vector2(5, 5));
        }

        [FlowTestCase(FlowTestCase.Horizontal)]
        private void test2()
        {
            buildTest(FillDirection.Horizontal, new Vector2(5, 5));
        }

        [FlowTestCase(FlowTestCase.Vertical)]
        private void test3()
        {
            buildTest(FillDirection.Vertical, new Vector2(5, 5));
        }

        private class TestCaseDropdownHeader : DropdownHeader
        {
            private readonly SpriteText label;

            protected override string Label
            {
                get { return label.Text; }
                set { label.Text = value; }
            }

            public TestCaseDropdownHeader()
            {
                Foreground.Padding = new MarginPadding(4);
                BackgroundColour = new Color4(100, 100, 100, 255);
                BackgroundColourHover = Color4.HotPink;
                Children = new[]
                {
                    label = new SpriteText(),
                };
            }
        }

        private class AnchorDropdown : Dropdown<Anchor>
        {
            protected override Menu CreateMenu() => new Menu();

            protected override DropdownHeader CreateHeader() => new TestCaseDropdownHeader();

            protected override DropdownMenuItem<Anchor> CreateMenuItem(string key, Anchor value) => new AnchorDropdownMenuItem(value);
        }

        private class AnchorDropdownMenuItem : DropdownMenuItem<Anchor>
        {
            public AnchorDropdownMenuItem(Anchor anchor)
                : base(anchor.ToString(), anchor)
            {
                AutoSizeAxes = Axes.Y;
                Foreground.Padding = new MarginPadding(2);

                Children = new[]
                {
                    new SpriteText { Text = anchor.ToString() },
                };
            }
        }

        private class FillDirectionDropdown : Dropdown<FlowTestCase>
        {
            protected override Menu CreateMenu() => new Menu();

            protected override DropdownHeader CreateHeader() => new TestCaseDropdownHeader();

            protected override DropdownMenuItem<FlowTestCase> CreateMenuItem(string key, FlowTestCase value) => new FillDirectionDropdownMenuItem(value);
        }

        private class FillDirectionDropdownMenuItem : DropdownMenuItem<FlowTestCase>
        {
            public FillDirectionDropdownMenuItem(FlowTestCase testCase)
                : base(testCase.ToString(), testCase)
            {
                AutoSizeAxes = Axes.Y;
                Foreground.Padding = new MarginPadding(2);

                Children = new[]
                {
                    new SpriteText { Text = testCase.ToString() },
                };
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class FlowTestCaseAttribute : Attribute
        {
            public FlowTestCase TestCase { get; }

            public FlowTestCaseAttribute(FlowTestCase testCase)
            {
                TestCase = testCase;
            }
        }

        private enum FlowTestCase
        {
            Full,
            Horizontal,
            Vertical,
        }
    }
}
