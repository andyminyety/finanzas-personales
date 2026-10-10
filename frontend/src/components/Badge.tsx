export default function Badge({ active }: { active: boolean }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-semibold ${
        active ? 'bg-brand-soft text-brand-text' : 'bg-danger-soft text-danger'
      }`}
    >
      <span className={`size-1.5 rounded-full ${active ? 'bg-brand' : 'bg-danger'}`} />
      {active ? 'Activo' : 'Inhabilitado'}
    </span>
  )
}
