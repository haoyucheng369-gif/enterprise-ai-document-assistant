import type { ChatRequest, ChatResponse } from '../types'

const defaultApiBaseUrl = 'http://localhost:5221'

function getApiBaseUrl() {
  return import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl
}

export async function sendChatMessage(
  request: ChatRequest,
  signal?: AbortSignal,
): Promise<ChatResponse> {
  const apiBaseUrl = getApiBaseUrl()
  const response = await fetch(`${apiBaseUrl}/api/chat`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
    signal,
  })

  if (!response.ok) {
    throw new Error(`Chat request failed with ${response.status}`)
  }

  return response.json() as Promise<ChatResponse>
}

export async function streamChatMessage(
  request: ChatRequest,
  onChunk: (chunk: string) => void,
  signal?: AbortSignal,
): Promise<void> {
  const apiBaseUrl = getApiBaseUrl()
  const response = await fetch(`${apiBaseUrl}/api/chat/stream`, {
    method: 'POST',
    headers: {
      Accept: 'text/plain',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
    signal,
  })

  if (!response.ok) {
    throw new Error(`Streaming chat request failed with ${response.status}`)
  }

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
