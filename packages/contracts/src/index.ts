export type ChatRole = "user" | "assistant";

export interface ChatMessage {
  id: string;
  requestId: string;
  role: ChatRole;
  content: string;
  createdAt: string;
}

export type AgUiEvent =
  | { type: "TEXT_MESSAGE_CONTENT"; messageId: string; delta: string }
  | { type: "TOOL_CALL_START"; toolCallId: string; toolName: string }
  | { type: "TOOL_CALL_END"; toolCallId: string; result: unknown }
  | { type: "RUN_ERROR"; message: string };

export interface TransactionDto {
  id: string;
  amount: number;
  category: string;
  note: string | null;
  occurredAt: string;
}

export interface PlanTaskDto {
  id: string;
  title: string;
  dueAt: string | null;
  estimatedMinutes: number | null;
  importance: "low" | "normal" | "high";
  done: boolean;
  overdue: boolean;
}

export interface IdeaNoteDto {
  id: string;
  content: string;
  createdAt: string;
}