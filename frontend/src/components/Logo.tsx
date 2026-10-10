interface Props {
  className?: string
}

/** App mark: a coin with an "F". Same artwork as public/favicon.svg. */
export default function Logo({ className = 'size-9' }: Props) {
  return (
    <svg viewBox="0 0 64 64" className={className} role="img" aria-label="Finanzas Personales" xmlns="http://www.w3.org/2000/svg">
      <rect width="64" height="64" rx="15" fill="#0e6b5c" />
      <circle cx="32" cy="32" r="21" fill="none" stroke="#ffffff" strokeWidth="4" />
      <rect x="25" y="21" width="5.5" height="22" rx="1.5" fill="#ffffff" />
      <rect x="25" y="21" width="15" height="5.5" rx="1.5" fill="#ffffff" />
      <rect x="25" y="30" width="11" height="5" rx="1.5" fill="#ffffff" />
    </svg>
  )
}
