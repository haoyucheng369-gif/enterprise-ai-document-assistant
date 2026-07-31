import type { ChatRequest, ChatResponse, UserIdentity } from '../types'
import { apiBaseUrl, buildUserHeaders, ensureSuccess } from './apiClient'

export async function sendChatMessage(
  request: ChatRequest,
  userId: UserIdentity,
  signal?: AbortSignal,
): Promise<ChatResponse> {
  const response = await fetch(`${apiBaseUrl}/api/chat`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
    signal,
  })

  await ensureSuccess(response, 'Chat request')

  return response.json() as Promise<ChatResponse>
}

export async function streamChatMessage(
  request: ChatRequest,
  userId: UserIdentity,
  onChunk: (chunk: string) => void,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(`${apiBaseUrl}/api/chat/stream`, {
    method: 'POST',
    headers: buildUserHeaders(userId, {
      Accept: 'text/plain',
      'Content-Type': 'application/json',
    }),
    body: JSON.stringify(request),
    signal,
  })

  await ensureSuccess(response, 'Streaming chat request')

  if (response.body === null) {
    onChunk(await response.text())
    return
  }

  const reader = response.body.getReader()
  const decoder = new TextDecoder()

  while (true) {
    const { done, value } = await reader.read()

    if (done) {
      const remainingText = decoder.decode()
      if (remainingText.length > 0) {
        onChunk(remainingText)
      }
      return
    }

    onChunk(decoder.decode(value, { stream: true }))
  }
}
