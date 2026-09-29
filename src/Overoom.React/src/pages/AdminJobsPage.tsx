import { ReactElement } from 'react';

import JobsModule from '../modules/admin/jobs-module/JobsModule.tsx';
import AuthorizeGuard from '../modules/guards/AuthorizeGuard.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Страница администрирования фоновых задач загрузки фильмов. Доступна только администраторам
 * @returns {ReactElement} JSX-элемент страницы
 */
const AdminJobsPage = (): ReactElement => {
  return (
    <AuthorizeGuard role="admin">
      <BlockTitle title="Фоновые задачи" />
      <JobsModule />
    </AuthorizeGuard>
  );
};

export default AdminJobsPage;
