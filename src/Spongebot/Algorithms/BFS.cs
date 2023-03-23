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

        public BFS(Board board)
        {
            this.board = board;
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

        private bool cellIsVisited(Cell cell, BFSPath path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (cell == path[i])
                    return true;
            }
            return false;
        }

        public async void runNonTSP()
        {
            board.clearColors();

            BFSPath initialPath = new BFSPath(startCell);
            List<BFSPath> completePath = new List<BFSPath> { initialPath };
            HashSet<Cell> unvisitedTreasure = new HashSet<Cell>(treasureCells);

            Queue<BFSPath> pathQ = new Queue<BFSPath>();
            pathQ.Enqueue(initialPath);

            while (pathQ.Count != 0)
            {
                BFSPath currentPath = pathQ.Dequeue();
                Cell lastCell = currentPath[currentPath.Length - 1];

                foreach (var path in completePath)
                {
                    path.stepColor();
                }
                currentPath.stepColor();

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                board.clearColors();

                if (unvisitedTreasure.Contains(lastCell))
                {
                    completePath.Add(currentPath);
                    pathQ.Clear();
                    unvisitedTreasure.Remove(lastCell);

                    currentPath = new BFSPath();
                }

                if (unvisitedTreasure.Count == 0)
                {
                    foreach (var path in completePath)
                    {
                        for (int i = 0; i < path.Length; i++)
                        {
                            path[i].finalPathVisitedColor();
                            await Task.Delay(TimeSpan.FromMilliseconds(500));
                        }
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
                        pathQ.Enqueue(new BFSPath(currentPath, neighbor));
                    }
                }
            }
        }

        public async void runTSP()
        {
            board.clearColors();
            BFSPath initialPath = new BFSPath(startCell);

            Queue<BFSPath> pathQ = new Queue<BFSPath>();
            pathQ.Enqueue(initialPath);

            while (pathQ.Count != 0)
            {
                BFSPath currentPath = pathQ.Dequeue();
                Cell lastCell = currentPath[currentPath.Length - 1];

                currentPath.stepColor();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                currentPath.clearColor();

                if (currentPath.treasureCount == treasureCells.Count)
                {
                    for (int i = 0; i < currentPath.Length; i++)
                    {
                        currentPath[i].finalPathVisitedColor();
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
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
                        pathQ.Enqueue(new BFSPath(currentPath, neighbor));
                    }
                }

                // FIX IF VISITS NEIGHBOR
                if (!hasNeighborToVisit)
                {
                    currentPath.prevCells.Pop();
                    Cell previous = currentPath.prevCells.Pop();
                    pathQ.Enqueue(new BFSPath(currentPath, previous));
                }
            }
        }
    }
}