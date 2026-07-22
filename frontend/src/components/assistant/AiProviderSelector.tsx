import type { AiProviderSelection } from '../../types'

type AiProviderSelectorProps = {
  selectedProvider: AiProviderSelection
  onSelectProvider: (provider: AiProviderSelection) => void
}

const providerOptions: Array<{
  value: AiProviderSelection
  label: string
}> = [
  {
    value: 'Mock',
    label: 'Mock',
  },
  {
    value: 'OpenAI',
    label: 'OpenAI',
  },
  {
    value: 'AzureOpenAI',
    label: 'Azure',
  },
]

export function AiProviderSelector({
  selectedProvider,
  onSelectProvider,
}: AiProviderSelectorProps) {
  return (
    <fieldset className="min-w-0">
      <legend className="sr-only">AI provider</legend>
      <div className="inline-grid grid-cols-3 gap-1 rounded-md border border-slate-200 bg-slate-50 p-1 text-xs">
        {providerOptions.map((option) => {
          const isSelected = option.value === selectedProvider

          return (
            <label
              className={`cursor-pointer rounded-sm border px-2.5 py-1.5 text-center font-medium transition ${
                isSelected
                  ? 'border-blue-300 bg-white text-blue-700 shadow-sm'
                  : 'border-transparent text-slate-500 hover:border-slate-200 hover:bg-white hover:text-slate-700'
              }`}
              key={option.value}
            >
              <input
                checked={isSelected}
                className="sr-only"
                name="ai-provider"
                onChange={() => onSelectProvider(option.value)}
                type="radio"
                value={option.value}
              />
              {option.label}
            </label>
          )
        })}
      </div>
    </fieldset>
  )
}
