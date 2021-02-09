// TS3AudioBot - An advanced Musicbot for Teamspeak 3
// Copyright (C) 2017  TS3AudioBot contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Open Software License v. 3.0
//
// You should have received a copy of the Open Software License along with this
// program. If not, see <https://opensource.org/licenses/OSL-3.0>.

namespace TS3AudioBot.CommandSystem.Commands
{
	using CommandResults;
	using System.Collections.Generic;

	public class StringCommand : ICommand
	{
		private readonly string content;
		private readonly string tailContent;

		public StringCommand(string content, string tailContent = null)
		{
			this.content = content;
			this.tailContent = tailContent;
		}

		public virtual ICommandResult Execute(ExecutionInformation info, IReadOnlyList<ICommand> arguments, IReadOnlyList<CommandResultType> returnTypes)
		{
			if (tailContent != null)
				return new TailStringCommandResult(content, tailContent);
			return new StringCommandResult(content);
		}

		public override string ToString() => $"S\"{content}\"";
	}
}
