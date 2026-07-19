import type { AiProviderSelection } from '../../types'

type AiProviderSelectorProps = {
  selectedProvider: AiProviderSelection
  onSelectProvider: (provider: AiProviderSelection) => void
}

const providerOptions: Array<{
  value: AiProviderSelection
  label: string
  description: string
}> = [
  {
    value: 'Mock',
    label: 'Local mock',
    description: 'No API cost',
  },
  {
    value: 'OpenAI',
    label: 'OpenAI',
    description: 'Real model',
  },
  {
    value: 'AzureOpenAI',
    label: 'Microsoft OpenAI',
    description: 'Azure provider',
  },
]

export function AiProviderSelector({
  selectedProvider,
  onSelectProvider,
}: AiProviderSelectorProps) {
  return (
    <fieldset className="rounded-md border border-slate-200 bg-slate-50 p-1.5">
      <legend className="sr-only">AI provider</legend>
      <div className="grid grid-cols-3 gap-1">
        {providerOptions.map((option) => {
          const isSelected = option.value === selectedProvider

          return (
            <label
              className={`cursor-pointer rounded-md border px-2 py-1.5 text-xs transition ${
                isSelected
                  ? 'border-blue-300 bg-white text-blue-700 shadow-sm'
                  : 'border-transparent text-slate-600 hover:border-slate-200 hover:bg-white'
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
              <span className="block font-semibold">{option.label}</span>
              <span className="block text-[11px] text-slate-400">
                {option.description}
              </span>
            </label>
          )
        })}
      </div>
    </fieldset>
  )
}
