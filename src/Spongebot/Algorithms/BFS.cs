﻿using Spongebot.Objects;
using Spongebot.Enums;
using System.Collections.Generic;
using System.Windows.Media;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace Spongebot.Algorithms
{
    internal class BFS
    {
        private Board board;
        private Cell startCell;
        private List<Cell> treasureCells = new List<Cell>();
        public string finalRoute = new String("");
        public int visitedNodes;
        public int totalSteps;

        public BFS(Board board)
        {
            this.board = board;
            this.visitedNodes = 0;
            this.totalSteps = 0;
            this.finalRoute = "";
            startCell = null!;
            for (int x = 0; x < board.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < board.Cells.GetLength(1); y++)
                {
                    if (board.Cells[x, y].Type == CellType.Start)
                        startCell = board.Cells[x, y];
                }
            }

            for (int x = 0; x < board.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < board.Cells.GetLength(1); y++)
                {
                    if (board.Cells[x, y].Type == CellType.Treasure)
                    {
                        treasureCells.Add(board.Cells[x, y]);
                    }
                }
            }
        }

        private bool cellIsVisited(Cell cell, MazePath path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (cell == path[i])
                    return true;
            }
            return false;
        }

        private string route(Cell cell1, Cell cell2)
        {
            if (cell1.Position.X == cell2.Position.X - 1)
            {
                return "R";
            }
            else if (cell1.Position.X == cell2.Position.X + 1)
            {
                return "L";
            }
            else if (cell1.Position.Y == cell2.Position.Y - 1)
            {
                return "D";
            }
            return "U";
        }

        public async Task runNonTSP(double timeInterval)
        {
            board.clearColors();

            MazePath initialPath = new MazePath(startCell);
            List<MazePath> completePath = new List<MazePath>();
            HashSet<Cell> unvisitedTreasure = new HashSet<Cell>(treasureCells);

            Queue<MazePath> pathQ = new Queue<MazePath>();
            pathQ.Enqueue(initialPath);

            this.visitedNodes = 0;
            this.totalSteps = 0;
            this.finalRoute = "";

            while (pathQ.Count != 0)
            {
                MazePath currentPath = pathQ.Dequeue();
                Cell lastCell = currentPath[currentPath.Length - 1];
                this.visitedNodes++;

                foreach (var path in completePath)
                {
                    path.stepColor();
                }
                currentPath.stepColor();
                lastCell.stepPathVisitingColor();

                await Task.Delay(TimeSpan.FromMilliseconds(timeInterval));

                board.clearColors();

                if (unvisitedTreasure.Contains(lastCell))
                {
                    completePath.Add(currentPath);
                    pathQ.Clear();
                    unvisitedTreasure.Remove(lastCell);

                    currentPath = new MazePath();
                    Debug.WriteLine($"lastCell: ({lastCell.Position.X}, {lastCell.Position.Y})");
                }

                if (unvisitedTreasure.Count == 0)
                {
                    Cell? previousLastCell = null;
                    foreach (var path in completePath)
                    {
                        for (int i = 0; i < path.Length; i++)
                        {
                            path[i].finalPathVisitedColor();
                            await Task.Delay(TimeSpan.FromMilliseconds(timeInterval));
                            this.totalSteps++;

                            if (i == 0 && previousLastCell != null)
                            {
                                this.finalRoute += route(previousLastCell, path[i]);
                            }
                            else if (i != 0)
                            {
                                this.finalRoute += route(path[i - 1], path[i]);
                            }

                            if (!(i == 0 && path == completePath[0]) &&
                                !(i == path.Length - 1 && path == completePath[completePath.Count - 1]))
                                this.finalRoute += " - ";
                        }
                        previousLastCell = path[path.Length - 1];
                    }
                    return;
                }

                Point[] neighborPositions = new Point[]
                {
                    new Point(lastCell.Position.X, lastCell.Position.Y - 1),
                    new Point(lastCell.Position.X + 1, lastCell.Position.Y),
                    new Point(lastCell.Position.X, lastCell.Position.Y + 1),
                    new Point(lastCell.Position.X - 1, lastCell.Position.Y)
                };

                foreach (var neighborPosition in neighborPositions)
                {
                    if (board.isValidPosition(neighborPosition) && !cellIsVisited(board[neighborPosition], currentPath) && board[neighborPosition].Type != CellType.Wall)
                    {
                        Cell neighbor = board[neighborPosition];
                        pathQ.Enqueue(new MazePath(currentPath, neighbor));
                    }
                }
            }
        }

        public async Task runTSP(double timeInterval)
        {
            board.clearColors();
            MazePath initialPath = new MazePath(startCell);

            Queue<MazePath> pathQ = new Queue<MazePath>();
            pathQ.Enqueue(initialPath);

            this.visitedNodes = 0;
            this.totalSteps = 0;
            this.finalRoute = "";

            while (pathQ.Count != 0)
            {
                MazePath currentPath = pathQ.Dequeue();
                Cell lastCell = currentPath[currentPath.Length - 1];
                this.visitedNodes++;

                currentPath.stepColor();
                lastCell.stepPathVisitingColor();
                await Task.Delay(TimeSpan.FromMilliseconds(timeInterval));
                currentPath.clearColor();

                if (currentPath.treasureCount == treasureCells.Count && lastCell.Type == CellType.Start)
                {
                    this.totalSteps = currentPath.Length;
                    for (int i = 0; i < currentPath.Length; i++)
                    {
                        currentPath[i].finalPathVisitedColor();
                        await Task.Delay(TimeSpan.FromMilliseconds(timeInterval));
                        if (i > 1)
                            this.finalRoute += " - ";
                        if (i > 0)
                            this.finalRoute += route(currentPath[i - 1], currentPath[i]);
                    }
                    return;
                }

                Point[] neighborPositions = new Point[]
                {
                    new Point(lastCell.Position.X, lastCell.Position.Y - 1),
                    new Point(lastCell.Position.X + 1, lastCell.Position.Y),
                    new Point(lastCell.Position.X, lastCell.Position.Y + 1),
                    new Point(lastCell.Position.X - 1, lastCell.Position.Y)
                };

                bool hasNeighborToVisit = false;
                foreach (var neighborPosition in neighborPositions)
                {
                    if (board.isValidPosition(neighborPosition) && !cellIsVisited(board[neighborPosition], currentPath) && board[neighborPosition].Type != CellType.Wall)
                    {
                        hasNeighborToVisit = true;
                        Cell neighbor = board[neighborPosition];
                        pathQ.Enqueue(new MazePath(currentPath, neighbor));
                    }
                }

                if (!hasNeighborToVisit)
                {
                    currentPath.prevCells.Pop();
                    Cell previous = currentPath.prevCells.Pop();
                    pathQ.Enqueue(new MazePath(currentPath, previous));
                }
            }
        }
    }
}