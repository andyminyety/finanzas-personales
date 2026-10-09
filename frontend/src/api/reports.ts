import api from './axios'
import type { Report, ReportFilters, ReportKind } from '../types'

export const getReport = (kind: ReportKind, filters: ReportFilters) => {
  const params = Object.fromEntries(Object.entries(filters).filter(([, v]) => v !== undefined && v !== ''))
  return api.get<Report>(`/reports/${kind}`, { params }).then((r) => r.data)
}
