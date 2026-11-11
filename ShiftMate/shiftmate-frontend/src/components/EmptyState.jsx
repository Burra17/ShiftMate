import { Link } from 'react-router-dom';

/**
 * EmptyState — Delad komponent för tomma listor.
 * Visar ikon, meddelande och valfri länk (concurrency test 2)
 */
const EmptyState = ({ icon = '📋', message, linkTo, linkText }) => {
  return (
    <div className="bg-slate-900/50 p-10 rounded-2xl text-center border border-dashed border-slate-800">
      <p className="text-3xl mb-3">{icon}</p>
      <p className="text-slate-400 font-medium text-sm">{message}</p>
      {linkTo && linkText && (
        <Link
          to={linkTo}
          className="text-blue-400 text-sm font-bold hover:underline mt-2 inline-block"
        >
          {linkText}
        </Link>
      )}
    </div>
  );
};

export default EmptyState;
