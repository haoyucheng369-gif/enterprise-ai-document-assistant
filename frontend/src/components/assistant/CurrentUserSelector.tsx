import { UserRound } from 'lucide-react'
import type { UserIdentity } from '../../types'

type CurrentUserSelectorProps = {
  selectedUser: UserIdentity
  onSelectUser: (userId: UserIdentity) => void
}

const userOptions: Array<{ value: UserIdentity; label: string }> = [
  { value: 'local-user', label: 'Local' },
  { value: 'alice', label: 'Alice' },
  { value: 'bob', label: 'Bob' },
  { value: 'charlie', label: 'Charlie' },
]

export function CurrentUserSelector({
  selectedUser,
  onSelectUser,
}: CurrentUserSelectorProps) {
  return (
    <fieldset className="min-w-0">
      <legend className="sr-only">Current user</legend>
      <div className="inline-grid grid-cols-[auto_repeat(4,minmax(0,1fr))] gap-1 rounded-md border border-slate-200 bg-slate-50 p-1 text-xs">
        <span
          aria-hidden="true"
          className="grid place-items-center px-1 text-emerald-600"
          title="Current user"
        >
          <UserRound size={14} />
        </span>
        {userOptions.map((option) => {
          const isSelected = option.value === selectedUser

          return (
            <label
              className={`cursor-pointer rounded-sm border px-2 py-1.5 text-center font-medium transition ${
                isSelected
                  ? 'border-emerald-300 bg-white text-emerald-700 shadow-sm'
                  : 'border-transparent text-slate-500 hover:border-slate-200 hover:bg-white hover:text-slate-700'
              }`}
              key={option.value}
            >
              <input
                checked={isSelected}
                className="sr-only"
                name="current-user"
                onChange={() => onSelectUser(option.value)}
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
