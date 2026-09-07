# About Pull Request

This repository is read-only, so Pull Request is not accepted. Thank you for your understanding.

# Caution

If you use this software for commercial, you must pay fees to NVIDIA.

# Oak

Oak is a go engine.

This software uses follwing technologies.

(1) Convolutional Neural Network

(2) Monte Carlo Tree Search

Both (1) and (2) are written by C#.

# Differences from Achernar.

Achernar uses PyTorch for prediction. Oak uses TorchSharp.

## Convolutional Neural Network

Convolutional Neural Network is composed of Policy Network and Value Network. Policy Network predicts best move of current position. Value Network evaluates current position.

### Input Features of Neural Network

(1) position of stones

(2) empty square

(3) previous n moves

(4) turn

### Output Labels of Policy Neural Network

square of move to

### Output Labels of Value Neural Network

win or lost

## Learning functions

Learning functions are also written by C# and TorchSharp. Policy Neural Network learns multi task classification. Value Neural Network learns binary classification.

### Record format for learning

Record format for learning is as below.

black_player,white_player,W+Resign,qd,dc,dq,pq,ce,fd,qo,oc,op, ...

First and second columns are players. Third column is game result. The following columns are moves. When learning Value Neural Network, you use third column. And learning Policy Neural Network, you use columns from third column to last column.

This software use the records on the internet download by myself.

## Operating environment

(1) OS: Windows 11 Pro

(2) Memory: 16GB or more. About 32GB is recommended.

(3) Memory usage on C# side: Less than 200MB when the MCTS task is 8.

(4) .NET Version: .NET 8.0

(5) Memory usage on the PyTorch side: About 4.4 GB. It is slightly heavy.

(6) TorchSharp-cuda-windows version: 0.105.0

## Known problems

(1) On the play out function in MCTS, Rollout policy is not implemented.

(2) Compared to top-class software, the recognition accuracy of Policy Network is poor.

(3) This software does not support GTP.

(4) If you create an executable file in a release build, you will get a deadlock when searching.

(5) Compared to Policy Network, Value Network is not accurate.

## How to use

If you navigate to the execution environment folder and execute the start.bat, the specified game record will be analyzed. The command-line arguments are as follows:

(1) Executing mode. "a" means analyzing. "s" means self playing.

(2) Date of the game

(3) Title name

(4) Record file name for analyzing

(5) File name of the result of analyzing.

(6) Tasks for analyzing.

(7) Thinking time for one move.

(8) Self play game count.

(9) Console Output threshold for self playing.

* (8) and (9) is not using to analyze a record.

* As Value Network is not accurate, the quality of the records created by self playing is not good.

## References

I developed this software referring to the softwares as below.

dlshogi

As far as I know, the source code for dlshogi is currently not publicly available.

I developed this software referring to the books as below. All books are written in Japanese, so I write the name of the books in Japanese.

(1) 山岡忠夫(2018),『将棋AIで学ぶディープラーニング』マイナビ出版 

(2) 山岡忠夫、加納邦彦(2021), 『強い将棋ソフトの創りかた　Pythonで実装するディープラーニング将棋AI』マイナビ出版

(3) 大槻知史(著)、三宅陽一郎(監修)(2018), 『最強囲碁AI アルファ碁解体新書　増補改訂版』翔泳社

(4) 原田達也(2017), 機械学習プロフェッショナルシリーズ『画像認識』講談社
