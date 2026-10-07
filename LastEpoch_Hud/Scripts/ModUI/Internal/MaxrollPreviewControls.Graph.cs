using System;
using System.Collections.Generic;
using System.Linq;
using LastEpoch_Hud.Scripts.Core.BuildImport;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static partial class MaxrollPreviewControls
{
    static GameObject graphPanel,
        graphViewport,
        graphContent,
        graphDetailsViewport;
    static ScrollRect graphScroll,
        nodeScroll;
    static Text graphTitle,
        graphSummary,
        graphHint,
        nodeDetails;
    static MaxrollTreeView drawnView;
    static readonly List<int> GraphClicks = new();
    static readonly Dictionary<int, Button> NodeButtons = new();
    static int? chosenNode,
        hoveredNode;
    static float graphZoom = .7f,
        drawnZoom;
    static Vector2 drawnPadding;
    static Dictionary<string, string> graphDictionary;

    static void BuildGraph()
    {
        GraphClicks.Clear();
        NodeButtons.Clear();
        graphContent = null;
        ResetGraph();
        graphPanel = Panel(root, "PlannerTree", .21f, .062f, .98f, .651f);
        graphTitle = Label(graphPanel, .025f, .917f, .60f, .985f, 18);
        graphSummary = Label(graphPanel, .025f, .85f, .97f, .914f, 12);
        Button(graphPanel, "−", .62f, .924f, .69f, .98f, () => ZoomGraph(-.15f));
        Button(graphPanel, "Fit", .70f, .924f, .80f, .98f, FitGraph);
        Button(graphPanel, "+", .81f, .924f, .88f, .98f, () => ZoomGraph(.15f));
        Button(
            graphPanel,
            "Build Issues",
            .89f,
            .924f,
            .98f,
            .98f,
            () =>
            {
                showIssues = true;
                detailPage = 0;
                Render();
            }
        );
        graphViewport = Panel(graphPanel, "TreeViewport", .025f, .09f, .72f, .84f);
        graphViewport.AddComponent<RectMask2D>();
        graphScroll = graphViewport.AddComponent<ScrollRect>();
        graphScroll.viewport = graphViewport.GetComponent<RectTransform>();
        graphScroll.horizontal = graphScroll.vertical = true;
        graphScroll.movementType = ScrollRect.MovementType.Clamped;
        graphScroll.scrollSensitivity = 25;
        graphScroll.inertia = false;
        graphDetailsViewport = Panel(graphPanel, "NamedNodeDetails", .74f, .09f, .975f, .84f);
        graphDetailsViewport.AddComponent<RectMask2D>();
        nodeDetails = Label(graphDetailsViewport, 0, 0, 1, 1, 13);
        nodeScroll = graphDetailsViewport.AddComponent<ScrollRect>();
        nodeScroll.viewport = graphDetailsViewport.GetComponent<RectTransform>();
        nodeScroll.content = nodeDetails.GetComponent<RectTransform>();
        nodeScroll.horizontal = false;
        nodeScroll.vertical = true;
        nodeScroll.movementType = ScrollRect.MovementType.Clamped;
        nodeScroll.scrollSensitivity = 20;
        graphHint = Label(graphPanel, .025f, .012f, .72f, .073f, 11);
        graphPanel.SetActive(false);
    }

    static void ResetGraph()
    {
        drawnView = null;
        chosenNode = hoveredNode = null;
        graphZoom = .7f;
    }

    static void ZoomGraph(float change)
    {
        graphZoom = Math.Max(.12f, Math.Min(1.5f, graphZoom + change));
        DrawGraph(false);
    }

    static void FitGraph()
    {
        var view = SelectedView;
        if (view == null || view.Nodes.Count == 0)
            return;
        Canvas.ForceUpdateCanvases();
        var size = graphScroll.viewport.rect;
        float width = (view.Nodes.Max(n => n.X) - view.Nodes.Min(n => n.X)) * 1.5f + 180;
        float height = (view.Nodes.Max(n => n.Y) - view.Nodes.Min(n => n.Y)) * 1.5f + 100;
        graphZoom = Math.Max(.12f, Math.Min(1, Math.Min(size.width / width, size.height / height)));
        DrawGraph(true);
        if (!graphContent.IsNullOrDestroyed())
            graphScroll.content.anchoredPosition = Vector2.zero;
    }

    static void RenderGraph(bool visible)
    {
        graphPanel.SetActive(visible);
        if (!visible)
            return;
        var view = SelectedView;
        graphTitle.text = view?.Name ?? L("Unknown skill");
        graphSummary.text =
            view == null
                ? ""
                : L("Points")
                    + ": "
                    + view.TotalPoints
                    + (
                        view.Mastery.HasValue
                            ? "  •  " + L("Total passive points") + ": " + view.Snapshot.TotalPoints
                            : ""
                    );
        if (view?.HasUnknownRanks == true)
            graphSummary.text +=
                "  " + L("Some allocated nodes are missing from current planner data.");
        graphHint.text = L("Drag to pan. Zoom for names. Hover or click a node for details.");
        if (
            !ReferenceEquals(drawnView, view)
            || drawnZoom != graphZoom
            || !ReferenceEquals(graphDictionary, dictionary)
        )
            DrawGraph(true);
    }

    static void ClearGraphContent()
    {
        foreach (int id in GraphClicks)
            Clicks.Remove(id);
        GraphClicks.Clear();
        NodeButtons.Clear();
        if (!graphContent.IsNullOrDestroyed())
        {
            graphContent.SetActive(false);
            UnityEngine.Object.Destroy(graphContent);
        }
    }

    static void DrawGraph(bool center)
    {
        var view = SelectedView;
        bool preserveCenter =
            !center && ReferenceEquals(view, drawnView) && !graphContent.IsNullOrDestroyed();
        var oldCenter = preserveCenter
            ? (
                new Vector2(
                    graphScroll.viewport.rect.width / 2,
                    graphScroll.viewport.rect.height / 2
                )
                - graphScroll.content.anchoredPosition
                - drawnPadding
            ) / drawnZoom
            : Vector2.zero;
        ClearGraphContent();
        drawnView = view;
        drawnZoom = graphZoom;
        graphDictionary = dictionary;
        if (view == null || !view.Snapshot.IsDecoded || view.Nodes.Count == 0)
        {
            nodeDetails.text = L("Tree history could not be decoded.");
            return;
        }
        Canvas.ForceUpdateCanvases();
        float minX = view.Nodes.Min(n => n.X),
            minY = view.Nodes.Min(n => n.Y);
        float scale = 1.5f * graphZoom;
        float paddingX = Math.Max(
            90 * graphZoom,
            (graphScroll.viewport.rect.width - (view.Nodes.Max(n => n.X) - minX) * scale) / 2
        );
        float paddingY = Math.Max(
            50 * graphZoom,
            (graphScroll.viewport.rect.height - (view.Nodes.Max(n => n.Y) - minY) * scale) / 2
        );
        drawnPadding = new Vector2(paddingX, paddingY);
        var positions = view.Nodes.ToDictionary(
            n => n.Id,
            n => new Vector2((n.X - minX) * scale + paddingX, (n.Y - minY) * scale + paddingY)
        );
        graphContent = new GameObject("TreeGraph");
        graphContent.layer = graphViewport.layer;
        var content = graphContent.AddComponent<RectTransform>();
        content.SetParent(graphViewport.transform, false);
        content.anchorMin = content.anchorMax = content.pivot = Vector2.zero;
        content.localScale = Vector3.one;
        content.sizeDelta = new Vector2(
            Math.Max(graphScroll.viewport.rect.width, positions.Values.Max(p => p.x) + paddingX),
            Math.Max(graphScroll.viewport.rect.height, positions.Values.Max(p => p.y) + paddingY)
        );
        graphScroll.content = content;
        foreach (var node in view.Nodes)
        foreach (var requirement in node.Requirements)
            if (positions.TryGetValue(requirement.NodeId, out var from))
            {
                var to = positions[node.Id];
                var line = new GameObject("Connection");
                line.layer = graphContent.layer;
                var rect = line.AddComponent<RectTransform>();
                rect.SetParent(content, false);
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = (from + to) / 2;
                rect.sizeDelta = new Vector2((to - from).magnitude, 2);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.Euler(
                    0,
                    0,
                    (float)(Math.Atan2(to.y - from.y, to.x - from.x) * 180 / Math.PI)
                );
                var image = line.AddComponent<Image>();
                image.raycastTarget = false;
                image.color =
                    view.Rank(node.Id) > 0 && view.Rank(requirement.NodeId) >= requirement.Points
                        ? Gold
                        : new Color(.25f, .28f, .32f);
            }
        foreach (var node in view.Nodes)
        {
            int nodeId = node.Id;
            var button = Button(graphContent, "", 0, 0, 1, 1, () => SelectGraphNode(nodeId));
            GraphClicks.Add(button.GetInstanceID());
            NodeButtons.Add(nodeId, button);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = positions[nodeId];
            rect.sizeDelta = new Vector2(140 * graphZoom, 70 * graphZoom);
            var text = button.GetComponentInChildren<Text>(true);
            string name =
                graphZoom >= .55f ? node.Name
                : graphZoom >= .4f || node.MaximumPoints == 0 ? Initials(node.Name)
                : "";
            text.text =
                name
                + (
                    node.MaximumPoints > 0
                        ? "\n" + view.Rank(nodeId) + "/" + node.MaximumPoints
                        : ""
                );
            text.fontSize = Math.Max(10, (int)(14 * graphZoom));
            text.color = view.Rank(nodeId) > 0 ? Gold : new Color(.70f, .72f, .75f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }
        if (!chosenNode.HasValue || !positions.ContainsKey(chosenNode.Value))
            chosenNode = view
                .Nodes.OrderByDescending(n => view.Rank(n.Id))
                .ThenBy(n => n.Id)
                .First()
                .Id;
        if (center)
        {
            var at = positions[chosenNode.Value];
            content.anchoredPosition =
                new Vector2(
                    graphScroll.viewport.rect.width / 2,
                    graphScroll.viewport.rect.height / 2
                ) - at;
        }
        else if (preserveCenter)
            content.anchoredPosition =
                new Vector2(
                    graphScroll.viewport.rect.width / 2,
                    graphScroll.viewport.rect.height / 2
                )
                - oldCenter * graphZoom
                - drawnPadding;
        RefreshGraphSelection();
        ShowNode(chosenNode.Value);
    }

    static string Initials(string name) =>
        string.Concat(
            name.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Take(3)
                .Select(word => word.Substring(0, 1))
        );

    static void SelectGraphNode(int id)
    {
        chosenNode = id;
        hoveredNode = null;
        RefreshGraphSelection();
        ShowNode(id);
    }

    static void RefreshGraphSelection()
    {
        var view = SelectedView;
        if (view == null)
            return;
        foreach (var pair in NodeButtons)
            pair.Value.GetComponent<Image>().color =
                pair.Key == chosenNode ? new Color(.32f, .25f, .12f)
                : view.Rank(pair.Key) > 0 ? new Color(.20f, .18f, .12f)
                : Dark;
    }

    static void ShowNode(int id)
    {
        var view = SelectedView;
        if (view == null || !view.Definition.Nodes.TryGetValue(id, out var node))
            return;
        var lines = new List<string>
        {
            node.Name
                + (
                    node.MaximumPoints > 0 ? "  —  " + view.Rank(id) + "/" + node.MaximumPoints : ""
                ),
        };
        if (!string.IsNullOrWhiteSpace(node.Description))
            lines.Add(node.Description);
        if (node.Stats.Count > 0)
        {
            lines.Add(L("Base node values"));
            lines.AddRange(node.Stats);
        }
        if (!string.IsNullOrWhiteSpace(node.PointBonusDescription))
            lines.Add(node.PointBonusDescription);
        if (!string.IsNullOrWhiteSpace(node.ExtraText))
            lines.Add(node.ExtraText);
        if (node.Requirements.Count > 0)
            lines.Add(
                L("Requires any")
                    + ": "
                    + string.Join(
                        " / ",
                        node.Requirements.Select(r =>
                            (
                                view.Definition.Nodes.TryGetValue(r.NodeId, out var parent)
                                    ? parent.Name
                                    : L("Unknown node")
                            )
                            + " ("
                            + r.Points
                            + ")"
                        )
                    )
            );
        if (node.MasteryRequirement > 0 && view.Name != null)
            lines.Add(L("Mastery points required") + ": " + node.MasteryRequirement);
        nodeDetails.text = string.Join("\n", lines);
        var rect = nodeScroll.content;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(
            -16,
            Math.Max(nodeScroll.viewport.rect.height, nodeDetails.preferredHeight + 16)
        );
        rect.anchoredPosition = new Vector2(8, 0);
        nodeScroll.verticalNormalizedPosition = 1;
    }

    static void TickGraph()
    {
        if (graphPanel.IsNullOrDestroyed() || !graphPanel.activeSelf || SelectedView == null)
            return;
        var canvas = graphPanel.GetComponentInParent<Canvas>();
        var camera =
            canvas.IsNullOrDestroyed() || canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;
        var mouse = (Vector2)Input.mousePosition;
        int? hover = null;
        if (RectTransformUtility.RectangleContainsScreenPoint(graphScroll.viewport, mouse, camera))
            foreach (var pair in NodeButtons)
                if (
                    RectTransformUtility.RectangleContainsScreenPoint(
                        pair.Value.GetComponent<RectTransform>(),
                        mouse,
                        camera
                    )
                )
                {
                    hover = pair.Key;
                    break;
                }
        if (hover == hoveredNode)
            return;
        hoveredNode = hover;
        if (hover.HasValue || chosenNode.HasValue)
            ShowNode(hover ?? chosenNode.Value);
    }
}
